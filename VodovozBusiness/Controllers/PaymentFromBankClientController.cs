using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.Controllers
{
	public class PaymentFromBankClientController : IPaymentFromBankClientController
	{
		private readonly IPaymentItemsRepository _paymentItemsRepository;
		private readonly IOrderRepository _orderRepository;

		public PaymentFromBankClientController(IPaymentItemsRepository paymentItemsRepository, IOrderRepository orderRepository)
		{
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
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
			if(HasOrderUndeliveredStatus(order.OrderStatus) || order.PaymentType != PaymentType.cashless)
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
						paymentItem.Payment.UpdateAllocatedSum(uow, order.Id, 0);
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
		
		/// <summary>
		/// Возврат суммы на баланс клиента при отмене заказа с распределением
		/// Если есть распределение на этот заказ, то создаем новый платеж с распределенной суммой
		/// для ее возврата на баланс клиента и последующего перераспределения
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="orderId">Id заказа</param>
		public void ReturnAllocatedSumToClientBalance(IUnitOfWork uow, int orderId)
		{
			if(!HasAllocatedSum(uow, orderId, out var paymentItems, out var allocatedSum))
			{
				return;
			}
			
			CreateNewPaymentForReturnAllocatedSumToClientBalance(uow, orderId, allocatedSum, paymentItems);
		}

		private void CreateNewPaymentForReturnAllocatedSumToClientBalance(
			IUnitOfWork uow, int orderId, decimal allocatedSum, IList<PaymentItem> paymentItems)
		{
			var payment = paymentItems.Select(x => x.Payment).FirstOrDefault();

			if(payment == null)
			{
				return;
			}

			var newPayment = payment.CreatePaymentForReturnAllocatedSumToClientBalance(allocatedSum, orderId);
			
			uow.Save(newPayment);
		}
	}
}
