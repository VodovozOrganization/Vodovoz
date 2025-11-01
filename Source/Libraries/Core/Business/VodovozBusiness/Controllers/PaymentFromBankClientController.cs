using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;
using VodovozBusiness.Services;

namespace Vodovoz.Controllers
{
	public class PaymentFromBankClientController : IPaymentFromBankClientController
	{
		private readonly IPaymentItemsRepository _paymentItemsRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IPaymentService _paymentService;
		private readonly IPaymentSettings _paymentSettings;
		private readonly object _lockObject = new object();

		public PaymentFromBankClientController(
			IPaymentItemsRepository paymentItemsRepository,
			IOrderRepository orderRepository,
			IPaymentsRepository paymentsRepository,
			IPaymentService paymentService,
			IPaymentSettings paymentSettings
			)
		{
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
			_paymentSettings = paymentSettings ?? throw new ArgumentNullException(nameof(paymentSettings));
		}
		
		/// <summary>
		/// Обновление распределенной суммы на заказ
		/// Выполнение происходит только если заказ находится в неотмененном статусе и с типом оплаты Безнал,
		/// а также имеет распределенную на себя сумму.
		/// Если сумма заказа увеличилась, т.е. больше распределенной суммы - ставим статус Частично оплачен
		/// Иначе, если сумма заказа уменьшилась - корректируем распределенную сумму в соответствии с заказом и ставим в Оплачен
		/// Иначе просто ставим в оплачен, т.е. сумма заказа и распределенная сумма совпадают
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="order">Заказ</param>
		public void UpdateAllocatedSum(IUnitOfWork uow, Order order)
		{
			if(HasOrderUndeliveredStatus(order.OrderStatus) || order.PaymentType != PaymentType.Cashless)
			{
				return;
			}
			if(!HasAllocatedSum(uow, order.Id, out var paymentItems, out var allocatedSum))
			{
				return;
			}
			
			if(order.OrderSum > allocatedSum)
			{
				order.OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
			}
			else if(order.OrderSum < allocatedSum)
			{
				var diff = allocatedSum - order.OrderSum;
				
				foreach(var paymentItem in paymentItems)
				{
					if(paymentItem.Sum > diff)
					{
						paymentItem.UpdateSum(paymentItem.Sum - diff);
						uow.Save(paymentItem);
						break;
					}
					if(paymentItem.Sum <= diff)
					{
						paymentItem.Payment.RemovePaymentItem(paymentItem.Id);
						uow.Save(paymentItem.Payment);
						diff -= paymentItem.Sum;
					}
					if(diff == 0)
					{
						break;
					}
				}
				order.OrderPaymentStatus = OrderPaymentStatus.Paid;
			}
			else
			{
				order.OrderPaymentStatus = OrderPaymentStatus.Paid;
			}
		}

		/// <summary>
		/// Возвращаем распределенную сумму на заказ, если менялся тип оплаты с безнала на любой другой
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="order">Заказ</param>
		public void ReturnAllocatedSumToClientBalanceIfChangedPaymentTypeFromCashless(IUnitOfWork uow, Order order)
		{
			if(_orderRepository.GetCurrentOrderPaymentTypeInDB(uow, order.Id) == PaymentType.Cashless
				&& order.PaymentType != PaymentType.Cashless)
			{
				ReturnAllocatedSumToClientBalance(uow, order, RefundPaymentReason.ChangeOrderPaymentType);
			}
		}
		
		/// <summary>
		/// Возврат распределенной суммы на баланс клиента
		/// Если есть распределение на этот заказ, то создаем новый платеж с распределенной суммой
		/// для ее возврата на баланс клиента и последующего перераспределения
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="order">Заказ</param>
		/// <param name="refundPaymentReason">Причина возврата суммы на баланс</param>
		public void ReturnAllocatedSumToClientBalance(
			IUnitOfWork uow, 
			Order order, 
			RefundPaymentReason refundPaymentReason = RefundPaymentReason.OrderCancellation
		)
		{
			lock(_lockObject)
			{
				if(!HasAllocatedSum(uow, order.Id, out var paymentItems, out var allocatedSum))
				{
					return;
				}

				CreateNewPaymentForReturnAllocatedSumToClientBalance(uow, order, allocatedSum, paymentItems, refundPaymentReason);
			}
		}

		/// <summary>
		/// Отмена возврата суммы на баланс клиента со всеми распределениями, если они есть,
		/// при возвращении заказа по безналу в работу
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="order">Заказ</param>
		/// <param name="previousOrderStatus">Предыдущий статус заказа</param>
		public void CancelRefundedPaymentIfOrderRevertFromUndelivery(
			IUnitOfWork uow,
			Order order,
			OrderStatus previousOrderStatus
		)
		{
			if(HasOrderUndeliveredStatus(previousOrderStatus) && order.PaymentType == PaymentType.Cashless)
			{
				var notCancelledRefundedPayments = GetNotCancelledRefundedPayments(uow, order.Id);

				if(!notCancelledRefundedPayments.Any())
				{
					return;
				}

				foreach(var refundedPayment in notCancelledRefundedPayments)
				{
					var reason = $"Причина отмены: заказ №{order.Id} вернули в статус " +
						$"{order.OrderStatus.GetEnumTitle()} из {previousOrderStatus.GetEnumTitle()}";

					_paymentService.CancelAllocation(uow, refundedPayment, reason, false);					
					uow.Save(refundedPayment);
				}
				
				var cancelledPaymentItems =
					_paymentItemsRepository.GetCancelledPaymentItemsForOrderFromNotCancelledPayments(uow, order.Id);

				foreach(var paymentItem in cancelledPaymentItems)
				{
					paymentItem.ReturnFromCancelled();
					uow.Save(paymentItem);
				}

				var allocatedSum = cancelledPaymentItems.Sum(pi => pi.Sum);
				order.OrderPaymentStatus = allocatedSum >= order.OrderSum ? OrderPaymentStatus.Paid : OrderPaymentStatus.PartiallyPaid;
			}
		}

		/// <summary>
		/// Отмена платежа по запросу пользователя. Отменяются все распределения. Сумма платежа на баланс пользователя не возвращается.
		/// </summary>
		/// <param name="uow">uow</param>
		/// <param name="paymentId">Id платежа</param>
		/// <param name="needUpdateOrderPaymentStatus"><c>true</c> стататус заказа обновляется, <c>false</c> стататус заказа не обновляется</param>
		public void CancellPaymentWithAllocationsByUserRequest(IUnitOfWork uow, int paymentId, bool needUpdateOrderPaymentStatus = true)
		{
			var payment = uow.GetById<Payment>(paymentId);

			if(payment == null || payment.Status == PaymentState.Cancelled)
			{
				return;
			}
			var reason = "Причина отмены: по запросу пользователя";
			_paymentService.CancelAllocation(uow, payment, reason, true);

			if(payment.CashlessMovementOperation != null)
			{
				payment.CashlessMovementOperation.CashlessMovementOperationStatus = AllocationStatus.Cancelled;
			}

			uow.Save(payment);
			uow.Commit();
		}

		private bool HasOrderUndeliveredStatus(OrderStatus orderStatus)
		{
			return _orderRepository.GetUndeliveryStatuses().Contains(orderStatus);
		}
		
		private bool HasAllocatedSum(IUnitOfWork uow, int orderId, out IList<PaymentItem> paymentItems, out decimal allocatedSum)
		{
			paymentItems = _paymentItemsRepository.GetAllocatedPaymentItemsForOrder(uow, orderId);
			allocatedSum = paymentItems.Select(pi => pi.CashlessMovementOperation).Sum(cmo => cmo.Expense);

			return paymentItems.Any();
		}
		
		private IEnumerable<Payment> GetNotCancelledRefundedPayments(IUnitOfWork uow, int orderId)
		{
			return _paymentsRepository.GetNotCancelledRefundedPayments(uow, orderId);
		}

		private void CreateNewPaymentForReturnAllocatedSumToClientBalance(
			IUnitOfWork uow,
			Order order,
			decimal allocatedSum,
			IList<PaymentItem> paymentItems,
			RefundPaymentReason refundPaymentReason
		)
		{
			foreach(var paymentItem in paymentItems)
			{
				paymentItem.CancelAllocation();
				uow.Save(paymentItem);
			}

			var payment = paymentItems.Select(x => x.Payment).First();
			var orderSum = order.OrderSum;
			decimal sum;

			if(orderSum > 0)
			{
				sum = allocatedSum <= orderSum ? allocatedSum : orderSum;
			}
			else
			{
				sum = allocatedSum;
			}

			if(order.OrderPaymentStatus != OrderPaymentStatus.UnPaid)
			{
				order.OrderPaymentStatus = order.PaymentType == PaymentType.Cashless
					? OrderPaymentStatus.UnPaid
					: OrderPaymentStatus.None;
			}

			var profitCategory = uow.GetById<ProfitCategory>(_paymentSettings.OtherProfitCategoryId);
			var newPayment = payment.CreatePaymentForReturnAllocatedSumToClientBalance(sum, order.Id, refundPaymentReason, profitCategory);
			uow.Save(newPayment);
		}
	}

	public enum RefundPaymentReason
	{
		[Display(Name = "Отмена доставки/заказа")]
		OrderCancellation,
		[Display(Name = "Смена типа оплаты заказа")]
		ChangeOrderPaymentType
	}
}
