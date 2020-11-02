using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
    public interface ICashlessPaymentRepository
    {
        decimal GetCashlessMovementOperationsSumForOrder(IUnitOfWork uow, int id);
        
        IList<PaymentItem> GetPaymentItemsForOrder(IUnitOfWork uow, int orderId);
    }
}