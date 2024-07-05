﻿using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
	public class PaymentItemsRepository : IPaymentItemsRepository
	{
		public IList<PaymentItem> GetAllocatedPaymentItemsForOrder(IUnitOfWork uow, int orderId)
		{
			CashlessMovementOperation cashlessMovementOperationAlias = null;
			
			var paymentItems = uow.Session.QueryOver<PaymentItem>()
				.Inner.JoinAlias(pi => pi.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.Where(pi => pi.Order.Id == orderId)
				.And(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.List();

			return paymentItems;
		}
		
		public IList<PaymentItem> GetCancelledPaymentItemsForOrderFromNotCancelledPayments(IUnitOfWork uow, int orderId)
		{
			Payment paymentAlias = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;
			
			var paymentItems = uow.Session.QueryOver<PaymentItem>()
				.Inner.JoinAlias(pi => pi.Payment, () => paymentAlias)
				.Inner.JoinAlias(pi => pi.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.Where(pi => pi.Order.Id == orderId)
				.And(() => paymentAlias.Status != PaymentState.Cancelled)
				.And(pi => pi.PaymentItemStatus == AllocationStatus.Cancelled)
				.List();

			return paymentItems;
		}
		
		public decimal GetAllocatedSumForOrderWithoutCurrentPayment(IUnitOfWork uow, int orderId, int paymentId)
		{
			return uow.Session.QueryOver<PaymentItem>()
				.Where(pi => pi.Order.Id == orderId)
				.And(pi => pi.Payment.Id != paymentId)
				.And(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<PaymentItem>(pi => pi.Sum))
				.SingleOrDefault<decimal>();
		}
		
		public decimal GetAllocatedSumForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<PaymentItem>()
				.Where(pi => pi.Order.Id == orderId)
				.And(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<PaymentItem>(pi => pi.Sum))
				.SingleOrDefault<decimal>();
		}
	}
}
