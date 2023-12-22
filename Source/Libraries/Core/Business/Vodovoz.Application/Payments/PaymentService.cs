using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Application.Payments
{
	/// <summary>
	/// Сервис оплат
	/// </summary>
	public sealed class PaymentService
	{
		private readonly IGenericRepository<Payment> _paymentRepository;
		private readonly IGenericRepository<Order> _orderRepository;

		public PaymentService(
			IGenericRepository<Payment> paymentRepository,
			IGenericRepository<Order> orderRepository)
		{
			_paymentRepository = paymentRepository
				?? throw new ArgumentNullException(nameof(paymentRepository));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public decimal GetBalanceByCounterpartyAndOrganizationIds(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			int organizationId)
		{
			return (from cmo in unitOfWork.Session.Query<CashlessMovementOperation>()
					where cmo.CashlessMovementOperationStatus != AllocationStatus.Cancelled
					  && cmo.Counterparty.Id == counterpartyId
					  && cmo.Organization.Id == organizationId
					select new
					{
						cmo.Income,
						cmo.Expense
					})
					.Sum(x => x.Income - x.Expense);
		}

		public decimal GetTotalCashlessNotPaidOrdersSum(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			int organizationId,
			int closingDocumentDeliveryScheduleId)
		{
			var orderShippedStatuses = new OrderStatus [] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

			return (from order in unitOfWork.Session.Query<Order>()
					join counterpartyContract in unitOfWork.Session.Query<CounterpartyContract>()
					on order.Contract.Id equals counterpartyContract.Id
					join orderItem in unitOfWork.Session.Query<OrderItem>()
					on order.Id equals orderItem.Order.Id
					into orderItems
					where order.Client != null
						&& order.Client.Id == counterpartyId
						&& counterpartyContract.Organization.Id == organizationId
						&& orderShippedStatuses.Contains(order.OrderStatus)
						&& order.PaymentType == PaymentType.Cashless
						&& order.OrderPaymentStatus != OrderPaymentStatus.Paid
						&& order.DeliverySchedule.Id != closingDocumentDeliveryScheduleId
					from subItem in orderItems.DefaultIfEmpty()
					select subItem.ActualSum)
					.ToArray()
					.Sum();
		}

		public decimal GetTotalCashlessPartiallyPaidOrdersSum(
			IUnitOfWork unitOfWork,
			int counterpartyId,
			int organizationId,
			int closingDocumentDeliveryScheduleId)
		{
			var orderShippedStatuses = new OrderStatus[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

			return (from paymentItem in unitOfWork.Session.Query<PaymentItem>()
					join order in unitOfWork.Session.Query<Order>()
					on paymentItem.Order.Id equals order.Id
					join counterpartyContract in unitOfWork.Session.Query<CounterpartyContract>()
					on order.Contract.Id equals counterpartyContract.Id
					join cashlessMovementOperation in unitOfWork.Session.Query<CashlessMovementOperation>()
					on paymentItem.CashlessMovementOperation.Id equals cashlessMovementOperation.Id
					where order.Client != null
						&& order.Client.Id == counterpartyId
						&& counterpartyContract.Organization.Id == organizationId
						&& orderShippedStatuses.Contains(order.OrderStatus)
						&& order.PaymentType == PaymentType.Cashless
						&& order.OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid
						&& cashlessMovementOperation.CashlessMovementOperationStatus != AllocationStatus.Cancelled
						&& order.DeliverySchedule.Id != closingDocumentDeliveryScheduleId
					select cashlessMovementOperation.Expense)
					.ToArray()
					.Sum();
		}
	}
}
