using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;

namespace Vodovoz.EntityRepositories.Cash.Requests
{
    public class CashRequestRepository: ICashRequestRepository
    {
        public IList<CashRequestSumItem> GetCashRequestSumItemsForCashRequest(IUnitOfWork uow, int cashRequestId)
        {
            var result = uow.Session.QueryOver<CashRequestSumItem>()
                .Where(x => x.CashRequest.Id == cashRequestId)
                .List();
            return result;
        }
    }
}