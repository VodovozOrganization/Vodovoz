using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed class RouteListItemCashDistributionDocumentRepository : IRouteListItemCashDistributionDocumentRepository
	{
		public decimal GetDistributedAmountOnRouteList(IUnitOfWork uow, int routeListId)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteListItemCashDistributionDocument docAlias = null;

			var query = uow.Session.QueryOver(() => docAlias)
				.Left.JoinAlias(() => docAlias.RouteListItem, () => routeListItemAlias)
				.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Where(() => routeListAlias.Id == routeListId)
				.Select(Projections.Sum(() => docAlias.Amount))
				.SingleOrDefault<decimal>();

			return query;
		}

		public decimal GetDistributedAmountOnRouteListItem(IUnitOfWork uow, int routeListItemId)
		{
			RouteListItemCashDistributionDocument docAlias = null;

			var query = uow.Session.QueryOver(() => docAlias)
				.Where(x => x.RouteListItem.Id == routeListItemId)
				.Select(Projections.Sum(() => docAlias.Amount))
				.SingleOrDefault<decimal>();

			return query;
		}

		public decimal GetDistributedIncomeAmount(IUnitOfWork uow, int incomeId)
		{
			RouteListItemCashDistributionDocument docAlias = null;

			var query = uow.Session.QueryOver(() => docAlias)
				.Where(() => docAlias.Income.Id == incomeId)
				.Select(Projections.Sum(() => docAlias.Amount))
				.SingleOrDefault<decimal>();

			return query;
		}

		public IList<RouteListItemCashDistributionDocument> GetRouteListItemCashDistributionDocuments(IUnitOfWork uow, int incomeId)
		{
			RouteListItemCashDistributionDocument docAlias = null;

			var query = uow.Session.QueryOver(() => docAlias)
				.Where(() => docAlias.Income.Id == incomeId)
				.List();

			return query;
		}
	}
}
