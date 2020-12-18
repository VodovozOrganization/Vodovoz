using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Cash
{
    public class RouteListItemCashDistributionDocumentRepository : IRouteListItemCashDistributionDocumentRepository
    {
        public decimal GetDistributedAmountOnRouteList(IUnitOfWork uow, RouteList routeList)
        {
            RouteList routeListAlias = null;
            RouteListItem routeListItemAlias = null;
            RouteListItemCashDistributionDocument docAlias = null;

            var query = uow.Session.QueryOver(() => docAlias)
                .Left.JoinAlias(() => docAlias.RouteListItem, () => routeListItemAlias)
                .Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
                .Where(() => routeListAlias.Id == routeList.Id)
                .Select(Projections.Sum(() => docAlias.Amount))
                .SingleOrDefault<decimal>();

            return query;
        }
        
        public decimal GetDistributedAmountOnRouteListItem(IUnitOfWork uow, RouteListItem routeListItem)
        {
            RouteListItemCashDistributionDocument docAlias = null;

            var query = uow.Session.QueryOver(() => docAlias)
                .Where(x => x.RouteListItem.Id == routeListItem.Id)
                .Select(Projections.Sum(() => docAlias.Amount))
                .SingleOrDefault<decimal>();

            return query;
        }
    }
}