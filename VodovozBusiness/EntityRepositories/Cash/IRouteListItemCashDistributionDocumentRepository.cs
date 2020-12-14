using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Cash
{
    public interface IRouteListItemCashDistributionDocumentRepository
    {
        decimal GetDistributedAmountOnRouteList(IUnitOfWork uow, RouteList routeList);
        decimal GetDistributedAmountOnRouteListItem(IUnitOfWork uow, RouteListItem routeListItem);
    }
}