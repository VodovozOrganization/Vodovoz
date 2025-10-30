using DocumentFormat.OpenXml.Spreadsheet;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;
using Vodovoz.Settings.Delivery;
using VodovozBusiness.Domain.Operations;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Payments
{
	/// <summary>
	/// Сервис оплат
	/// </summary>
	internal sealed class PaymentService : IPaymentService
	{
		private const int _commentLimit = 300;
		private int _closingDocumentDeliveryScheduleId;

		private readonly IGenericRepository<Payment> _paymentRepository;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IPaymentItemsRepository _paymentItemsRepository;
		private readonly IPaymentSettings _paymentSettings;

		public PaymentService(
			IGenericRepository<Payment> paymentRepository,
			IGenericRepository<Order> orderRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IPaymentItemsRepository paymentItemsRepository
			IPaymentSettings paymentSettings)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentSettings = paymentSettings ?? throw new ArgumentNullException(nameof(paymentSettings));
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));

			_closingDocumentDeliveryScheduleId =
				(deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings)))
				.ClosingDocumentDeliveryScheduleId;
		}

		public Result DistributeByClientIdAndOrganizationId(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			int organizationId,
			bool distributeCompletedPayments = false)
		{
			var balance = GetCounterpartyBalanceByIdAndOrganizationId(
				unitOfWork,
				counterpartyId,
				organizationId);

			var paymentNodes = GetAllNotFullyAllocatedPaymentsByClientAndOrg(
				unitOfWork,
				counterpartyId,
				organizationId,
				distributeCompletedPayments);

			var orderNodes = GetAllNotFullyPaidOrdersByClientAndOrgForAutomaticDistribution(
				unitOfWork,
				counterpartyId,
				organizationId);

			if(!orderNodes.Any())
			{
				return Result.Failure(Vodovoz.Errors.Payments.PaymentsDistributionErrors.NoOrdersToDistribute(counterpartyId));
			}

			if(!paymentNodes.Any())
			{
				return Result.Failure(Vodovoz.Errors.Payments.PaymentsDistributionErrors.NoPaymentsWithPositiveBalance(counterpartyId));
			}

			foreach(var paymentNode in paymentNodes)
			{
				try
				{
					if(balance == 0)
					{
						break;
					}

					var unallocatedSum = paymentNode.UnallocatedSum;
					var payment = _paymentRepository
						.Get(unitOfWork, p => p.Id == paymentNode.Id)
						.FirstOrDefault();

					while(orderNodes.Count > 0)
					{
						var order = _orderRepository
							.Get(unitOfWork, o => o.Id == orderNodes[0].Id)
							.FirstOrDefault();

						var sumToAllocate = orderNodes[0].OrderSum - orderNodes[0].AllocatedSum;

						if(balance >= unallocatedSum)
						{
							if(sumToAllocate <= unallocatedSum)
							{
								payment.AddPaymentItem(order, sumToAllocate);
								unallocatedSum -= sumToAllocate;
								balance -= sumToAllocate;
								orderNodes.RemoveAt(0);
								order.OrderPaymentStatus = OrderPaymentStatus.Paid;
							}
							else
							{
								payment.AddPaymentItem(order, unallocatedSum);
								orderNodes[0].AllocatedSum += unallocatedSum;
								balance -= unallocatedSum;
								order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
								break;
							}

							if(unallocatedSum == 0)
							{
								break;
							}
						}
						else
						{
							if(sumToAllocate <= balance)
							{
								payment.AddPaymentItem(order, sumToAllocate);
								balance -= sumToAllocate;
								orderNodes.RemoveAt(0);
								order.OrderPaymentStatus = OrderPaymentStatus.Paid;
							}
							else
							{
								payment.AddPaymentItem(order, balance);
								balance = 0;
								order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
							}

							if(balance == 0)
							{
								break;
							}
						}
					}

					var allocatedPaymentItems =
						payment.Items.Where(
							pi => pi.CashlessMovementOperation == null
								|| pi.Sum != pi.CashlessMovementOperation.Expense);

					foreach(var paymentItem in allocatedPaymentItems)
					{
						paymentItem.CreateOrUpdateExpenseOperation();
					}

					if(payment.Status != PaymentState.completed)
					{
						payment.CreateIncomeOperation();
						payment.Status = PaymentState.completed;
					}

					unitOfWork.Save(payment);
				}
				catch(Exception e)
				{
					return Result.Failure(Vodovoz.Errors.Payments.PaymentsDistributionErrors.AutomaticDistribution(e.Message));
				}
			}

			return Result.Success();
		}

		public Result<IEnumerable<UnallocatedBalancesJournalNode>> GetAllUnallocatedBalancesForAutomaticDistribution(IUnitOfWork unitOfWork)
		{
			var statuses = new OrderStatus[]
			{
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			var counterpartiesWithDebts =
				(from order in unitOfWork.Session.Query<Order>()
				 join counterpartyContract in unitOfWork.Session.Query<CounterpartyContract>()
				 on order.Contract.Id equals counterpartyContract.Id
				 let counterpartyDebt = (decimal?)order.OrderItems.Sum(oi => oi.ActualSum) ?? 0m
				 where order.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId
					 && statuses.Contains(order.OrderStatus)
					 && order.OrderPaymentStatus != OrderPaymentStatus.Paid
					 && order.PaymentType == PaymentType.Cashless
					 && counterpartyDebt > 0
				 select
					order.Client.Id).Distinct().ToList();

			var unallocatedNodes =
				(from payment in unitOfWork.Session.Query<Payment>()
				 join organization in unitOfWork.Session.Query<Organization>()
				 on payment.Organization.Id equals organization.Id
				 join counterparty in unitOfWork.Session.Query<Counterparty>()
				 on payment.Counterparty.Id equals counterparty.Id
				 let counterpartyBalance =
					 (decimal?)(from cashlessMovementOperation in unitOfWork.Session.Query<CashlessMovementOperation>()
								where cashlessMovementOperation.Counterparty.Id == counterparty.Id
									&& cashlessMovementOperation.Organization.Id == organization.Id
									&& cashlessMovementOperation.CashlessMovementOperationStatus != AllocationStatus.Cancelled
								select cashlessMovementOperation.Income - cashlessMovementOperation.Expense).Sum() ?? 0m
				 let notPaidOrdersSum =
					 (decimal?)(from order in unitOfWork.Session.Query<Order>()
								join counterpartyContract in unitOfWork.Session.Query<CounterpartyContract>()
								on order.Contract.Id equals counterpartyContract.Id
								where order.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId
									&& statuses.Contains(order.OrderStatus)
									&& order.OrderPaymentStatus != OrderPaymentStatus.Paid
									&& order.PaymentType == PaymentType.Cashless
									&& order.Client.Id == counterparty.Id
									&& counterpartyContract.Organization.Id == organization.Id
								select order.OrderItems.Sum(oi => oi.ActualSum)).Sum() ?? 0m
				 let notFullyPaidOrdersPaidSum =
					(decimal?)(from cashlessMovementOperationExpense in unitOfWork.Session.Query<CashlessMovementOperation>()
							   join paymentItem in unitOfWork.Session.Query<PaymentItem>()
							   on cashlessMovementOperationExpense.Id equals paymentItem.CashlessMovementOperation.Id
							   join notFullyPaidOrder in unitOfWork.Session.Query<Order>()
							   on paymentItem.Order.Id equals notFullyPaidOrder.Id
							   where statuses.Contains(notFullyPaidOrder.OrderStatus)
								&& notFullyPaidOrder.PaymentType == PaymentType.Cashless
								&& notFullyPaidOrder.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId
								&& notFullyPaidOrder.OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid
								&& cashlessMovementOperationExpense.Counterparty.Id == counterparty.Id
								&& cashlessMovementOperationExpense.Organization.Id == organization.Id
								&& cashlessMovementOperationExpense.CashlessMovementOperationStatus != AllocationStatus.Cancelled
							   select cashlessMovementOperationExpense.Expense).Sum() ?? 0m
				 let counterpartyDebt = notPaidOrdersSum - notFullyPaidOrdersPaidSum
				 where counterpartiesWithDebts.Contains(payment.Counterparty.Id)
					 && counterpartyDebt > 0
					 && counterpartyBalance > 0
				 orderby counterpartyBalance descending
				 select new UnallocatedBalancesJournalNode
				 {
					 OrganizationId = organization.Id,
					 OrganizationName = organization.Name,
					 CounterpartyId = counterparty.Id,
					 CounterpartyINN = counterparty.INN,
					 CounterpartyName = counterparty.Name,
					 CounterpartyBalance = counterpartyBalance,
					 CounterpartyDebt = counterpartyDebt
				 }).Distinct();

			var result = unallocatedNodes.ToList();

			return result;
		}

		private IEnumerable<NotFullyAllocatedPaymentNode> GetAllNotFullyAllocatedPaymentsByClientAndOrg(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			int organizationId,
			bool allocateCompletedPayments)
			=> (from payment in unitOfWork.Session.Query<Payment>()
				where payment.Counterparty.Id == counterpartyId
					&& payment.Organization.Id == organizationId
					&& payment.ProfitCategory.Id == _paymentSettings.DefaultProfitCategoryId 
					&& (allocateCompletedPayments
						? payment.Status == PaymentState.completed
						: payment.Status != PaymentState.Cancelled)
					&& (payment.Total - ((decimal?)payment.Items
						.Where(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
						.Sum(pi => pi.Sum) ?? 0m) > 0)
				orderby payment.Total - ((decimal?)payment.Items
					.Where(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
					.Sum(pi => pi.Sum) ?? 0m) descending
				orderby payment.Date ascending
				select new NotFullyAllocatedPaymentNode
				{
					Id = payment.Id,
					PaymentDate = payment.Date,
					UnallocatedSum = payment.Total - ((decimal?)payment.Items
						.Where(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
						.Sum(pi => pi.Sum) ?? 0m)
				}).ToList();

		private IList<NotFullyPaidOrderNode> GetAllNotFullyPaidOrdersByClientAndOrgForAutomaticDistribution(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			int organizationId)
		{
			var statuses = new OrderStatus[]
			{
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			return (from order in unitOfWork.Session.Query<Order>()
					where order.Client.Id == counterpartyId
						&& order.Contract.Organization.Id == organizationId
						&& statuses.Contains(order.OrderStatus)
						&& order.PaymentType == PaymentType.Cashless
						&& order.OrderPaymentStatus != OrderPaymentStatus.Paid
						&& order.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId
					let orderSum = (decimal?)order.OrderItems.Sum(oi => oi.ActualSum) ?? 0m
					let allocatedSum =
						(decimal?)(from paymentItem in unitOfWork.Session.Query<PaymentItem>()
								   join cashlessMovementOperation in unitOfWork.Session.Query<CashlessMovementOperation>()
								   on paymentItem.CashlessMovementOperation.Id equals cashlessMovementOperation.Id
								   where paymentItem.Order.Id == order.Id
										  && paymentItem.PaymentItemStatus != AllocationStatus.Cancelled
								   select cashlessMovementOperation.Expense).Sum() ?? 0m
					where orderSum > 0
					orderby order.DeliveryDate ascending
					orderby order.CreateDate ascending
					select new NotFullyPaidOrderNode
					{
						Id = order.Id,
						OrderDeliveryDate = order.DeliveryDate,
						OrderCreationDate = order.CreateDate,
						OrderSum = orderSum,
						AllocatedSum = allocatedSum
					})
				   .ToList();
		}

		private decimal GetCounterpartyBalanceByIdAndOrganizationId(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			int organizationId)
			=> (decimal?)(from cashlessMovementOperation in unitOfWork.Session.Query<CashlessMovementOperation>()
						  where cashlessMovementOperation.Counterparty.Id == counterpartyId
							  && cashlessMovementOperation.Organization.Id == organizationId
							  && cashlessMovementOperation.CashlessMovementOperationStatus != AllocationStatus.Cancelled
						  select cashlessMovementOperation.Income - cashlessMovementOperation.Expense)
					.Sum() ?? 0m;

		public void CancelAllocation(
			IUnitOfWork uow,
			Payment payment,
			string cancellationReason,
			bool isByUserRequest
			)
		{
			if(payment.IsRefundPayment || isByUserRequest)
			{
				payment.Status = PaymentState.Cancelled;
				payment.Comment += string.IsNullOrWhiteSpace(payment.Comment) 
					? $"{cancellationReason}" 
					: $"\n{cancellationReason}";

				if(payment.Comment.Length > _commentLimit)
				{
					payment.Comment = payment.Comment.Remove(_commentLimit);
				}
			}

			foreach(var paymentItem in payment.Items)
			{
				CancelAllocationWithUpdateOrderPayments(uow, paymentItem);
			}
		}

		public void CancelAllocationWithUpdateOrderPayments(IUnitOfWork uow, PaymentItem paymentItem)
		{
			paymentItem.CancelAllocation();
			UpdateCashlessOrderPaymentStatus(uow, paymentItem.Order, paymentItem.Sum);
		}

		public void UpdateCashlessOrderPaymentStatus(IUnitOfWork uow, Order order, decimal canceledSum)
		{
			var allocatedSum = _paymentItemsRepository.GetAllocatedSumForOrder(uow, order.Id);

			UpdateCashlessOrderPaymentStatusByAllocatedSum(order, canceledSum, allocatedSum);
		}

		public async Task UpdateCashlessOrderPaymentStatusAsync(
			IUnitOfWork uow,
			Order order,
			decimal canceledSum,
			CancellationToken cancellationToken
			)
		{
			var allocatedSum = await _paymentItemsRepository.GetAllocatedSumForOrderAsync(uow, order.Id, cancellationToken);

			UpdateCashlessOrderPaymentStatusByAllocatedSum(order, canceledSum, allocatedSum);
		}

		private void UpdateCashlessOrderPaymentStatusByAllocatedSum(
			Order order,
			decimal canceledSum,
			decimal allocatedSum
			)
		{
			var isUnpaid = allocatedSum == 0 || allocatedSum - canceledSum == 0;
			var isPaid = allocatedSum - canceledSum > order.OrderSum;

			if(isUnpaid)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.UnPaid;
			}
			else if(isPaid)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.Paid;
			}
			else
			{
				order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
			}
		}
	}
}
