using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class WayBillDocumentRepository : IWayBillDocumentRepository
	{
		public IList<Order> GetOrdersForWayBillDocuments(IUnitOfWork uow, DateTime startDate, DateTime endDate)
		{
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			Order orderAlias = null;
			return uow.Session.QueryOver(() => orderAlias)
				.JoinEntityAlias(() => routeListItemAlias, () => orderAlias.Id == routeListItemAlias.Order.Id, JoinType.InnerJoin)
				.JoinEntityAlias(() => routeListAlias, () => routeListItemAlias.RouteList.Id == routeListAlias.Id, JoinType.InnerJoin)
				.Where(() => routeListAlias.Status == RouteListStatus.Closed)
				.And(() => routeListAlias.Date >= startDate)
				.And(() => routeListAlias.Date <= endDate)
				.And(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.And(() => orderAlias.PaymentType == PaymentType.Cashless)
				.List();
		}
	}
}
