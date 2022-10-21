using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;

namespace Vodovoz.EntityRepositories.Cash.Requests
{
    public interface ICashRequestRepository
    {
        IList<CashRequestSumItem> GetCashRequestSumItemsForCashRequest(IUnitOfWork uow, int CashRequestId);
    }
}