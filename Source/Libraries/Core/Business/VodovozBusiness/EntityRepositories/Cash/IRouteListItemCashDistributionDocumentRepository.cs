using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Cash
{
    public interface IRouteListItemCashDistributionDocumentRepository
    {
        decimal GetDistributedAmountOnRouteList(IUnitOfWork uow, int routeListId);
        decimal GetDistributedIncomeAmount(IUnitOfWork uow, int incomeId);
        decimal GetDistributedAmountOnRouteListItem(IUnitOfWork uow, int routeListItemId);
        IList<RouteListItemCashDistributionDocument> GetRouteListItemCashDistributionDocuments(IUnitOfWork uow,
            int incomeId);
    }
}