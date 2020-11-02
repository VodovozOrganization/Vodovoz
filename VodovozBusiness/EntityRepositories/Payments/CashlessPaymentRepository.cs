using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
    public class CashlessPaymentRepository : ICashlessPaymentRepository
    {
        public decimal GetCashlessMovementOperationsSumForOrder(IUnitOfWork uow, int id)
        {
            if (uow == null) throw new ArgumentNullException(nameof(uow));
            PaymentReturn paymentReturnAlias = null;
            PaymentItem paymentItemAlias = null;
            CashlessMovementOperation cashlessMovementOperationAlias = null;

            var expence = uow.Session.QueryOver(() => paymentItemAlias)
                .Inner.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
                .Select(Projections.Sum(() => cashlessMovementOperationAlias.Expense))
                .Where(() => paymentItemAlias.Order.Id == id)
                .SingleOrDefault<decimal>();
            
            var income = uow.Session.QueryOver(() => paymentReturnAlias)
                .Inner.JoinAlias(() => paymentReturnAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
                .Select(Projections.Sum(() => cashlessMovementOperationAlias.Income))
                .Where(() => paymentReturnAlias.Order.Id == id)
                .SingleOrDefault<decimal>();
            
            return expence-income;
        }
        
        public IList<PaymentItem> GetPaymentItemsForOrder(IUnitOfWork uow, int orderId)
        {
            if (uow == null) throw new ArgumentNullException(nameof(uow));
            var paymentItems = uow.Session.QueryOver<PaymentItem>()
                .Where(x => x.Order.Id == orderId)
                .List();

            return paymentItems;
        }
    }
}
