using System;
using System.Collections.Generic;
using System.Linq;
using DateTimeHelpers;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Undeliveries
{
	internal sealed class UndeliveredOrdersRepository : IUndeliveredOrdersRepository
	{
		public Dictionary<GuiltyTypes, int> GetDictionaryWithUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrder undeliveredOrderAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver(() => undeliveredOrderAlias);

			if(start != null && end != null)
			{
				query.Left.JoinAlias(u => u.OldOrder, () => orderAlias)
					.Where(() => orderAlias.DeliveryDate >= start)
					.Where(u => orderAlias.DeliveryDate <= end);
			}

			var result = query
				.Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
				.SelectList(list => list
							.SelectGroup(() => guiltyInUndeliveryAlias.GuiltySide)
							.SelectCount(() => undeliveredOrderAlias.Id)
						   )
				.List<object[]>();

			return result.ToDictionary(x => (GuiltyTypes)x[0], x => (int)x[1]);
		}

		public IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrderCountNode resultAlias = null;
			UndeliveredOrder undeliveredOrderAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver(() => undeliveredOrderAlias)
						   .Left.JoinAlias(u => u.OldOrder, () => orderAlias);

			if(start != null && end != null)
				query.Where(() => orderAlias.DeliveryDate >= start)
					 .Where(u => orderAlias.DeliveryDate <= end);

			var result = query
				.Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
				.SelectList(list => list
							.SelectGroup(() => guiltyInUndeliveryAlias.GuiltySide).WithAlias(() => resultAlias.Type)
							.SelectCount(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.Count)
						   )
				.TransformUsing(Transformers.AliasToBean<UndeliveredOrderCountNode>())
				.List<UndeliveredOrderCountNode>();

			return result;
		}

		public IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountOnDptForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrderCountNode resultAlias = null;
			UndeliveredOrder undeliveredOrderAlias = null;
			Subdivision subdivisionAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver(() => undeliveredOrderAlias)
						   .Where(() => guiltyInUndeliveryAlias.GuiltySide == GuiltyTypes.Department)
						   .Left.JoinAlias(u => u.OldOrder, () => orderAlias)
						   .Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias)
						   .Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias);

			if(start != null && end != null)
				query.Where(() => orderAlias.DeliveryDate >= start)
					 .Where(u => orderAlias.DeliveryDate <= end);

			var result = query.SelectList(list => list
										  .SelectGroup(() => subdivisionAlias.Id).WithAlias(() => resultAlias.SubdivisionId)
										  .Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
										  .SelectCount(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.Count)
										 )
							  .TransformUsing(Transformers.AliasToBean<UndeliveredOrderCountNode>())
							  .List<UndeliveredOrderCountNode>();

			return result;
		}

		public IList<UndeliveredOrder> GetListOfUndeliveriesForOrder(IUnitOfWork uow, int orderId)
		{
			Order order = uow.GetById<Order>(orderId);
			return GetListOfUndeliveriesForOrder(uow, order);
		}

		public IList<UndeliveredOrder> GetListOfUndeliveriesForOrder(IUnitOfWork uow, Order order)
		{
			var query = uow.Session.QueryOver<UndeliveredOrder>()
						   .Where(u => u.OldOrder == order)
						   .List<UndeliveredOrder>();

			return query;
		}

		public IList<int> GetListOfUndeliveryIdsForDriver(IUnitOfWork uow, Employee driver)
		{
			Order orderAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;

			var query = uow.Session.QueryOver(() => routeListItemAlias)
						   .Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
						   .Where(() => routeListAlias.Driver == driver)
						   .Left.JoinQueryOver(() => routeListItemAlias.Order, () => orderAlias);

			var q = query.List().Select(i => i.Order.Id);

			return q.ToList();
		}

		public IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrder undeliveredOrderAlias = null;
			Subdivision subdivisionAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver(() => undeliveredOrderAlias)
							  .Left.JoinAlias(u => u.OldOrder, () => orderAlias)
							  .Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
							  .Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias);

			if(start != null && end != null)
			{
				query.Where(() => orderAlias.DeliveryDate >= start)
					.Where(u => orderAlias.DeliveryDate <= end);
			}

			int i = 0;
			var result = query.SelectList(list => list
				  .SelectGroup(u => u.Id)
				  .Select(
					  Projections.SqlFunction(
						  new SQLFunctionTemplate(
							  NHibernateUtil.String,
							  "GROUP_CONCAT(" +
							  "CASE ?1 " +
							  $"WHEN '{nameof(GuiltyTypes.Department)}' THEN IFNULL(CONCAT('Отд: ', ?2), '{GuiltyTypes.Department.GetEnumTitle()}') " +
							  $"WHEN '{nameof(GuiltyTypes.Client)}' THEN '{GuiltyTypes.Client.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.Driver)}' THEN '{GuiltyTypes.Driver.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.ServiceMan)}' THEN '{GuiltyTypes.ServiceMan.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.ForceMajor)}' THEN '{GuiltyTypes.ForceMajor.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.DirectorLO)}' THEN '{GuiltyTypes.DirectorLO.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.DirectorLOCurrentDayDelivery)}' THEN '{GuiltyTypes.DirectorLOCurrentDayDelivery.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.AutoСancelAutoTransfer)}' THEN '{GuiltyTypes.AutoСancelAutoTransfer.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.None)}' THEN '{GuiltyTypes.None.GetEnumTitle()}' " +
							  "ELSE ?1 " +
							  "END ORDER BY ?1 ASC SEPARATOR '\n')"
							 ),
						  NHibernateUtil.String,
						  Projections.Property(() => guiltyInUndeliveryAlias.GuiltySide),
						  Projections.Property(() => subdivisionAlias.ShortName)
						 )
					 )
				 )
		  .List<object[]>()
		  .GroupBy(x => x[1])
		  .Select(r => new[] { r.Key, r.Count(), i++ })
		  .ToList();

			return result;
		}

		public decimal GetUndelivered19LBottlesQuantity(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;

			var subquery = QueryOver.Of<UndeliveredOrder>()
				.Left.JoinAlias(u => u.OldOrder, () => orderAlias);

			if(start != null && end != null)
			{
				subquery.Where(() => orderAlias.DeliveryDate >= start)
					.Where(() => orderAlias.DeliveryDate <= end);
			}

			subquery.Select(u => u.OldOrder.Id);

			var bottles19L = uow.Session.QueryOver<OrderItem>()
				.WithSubquery.WhereProperty(i => i.Order.Id).In(subquery)
				.Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
				.Where(n => n.Category == NomenclatureCategory.water && n.TareVolume == TareVolume.Vol19L)
				.SelectList(list => list.SelectSum(i => i.Count))
				.List<decimal?>()
				.FirstOrDefault();

			return bottles19L ?? 0;
		}

		public Order GetOldOrderFromUndeliveredByNewOrderId(IUnitOfWork uow, int newOrderId)
		{
			Order oldOrderAlias = null;

			return uow.Session.QueryOver<UndeliveredOrder>()
				.JoinAlias(u => u.OldOrder, () => oldOrderAlias)
				.Where(u => u.NewOrder.Id == newOrderId)
				.Select(Projections.Entity(() => oldOrderAlias))
				.SingleOrDefault<Order>();
		}

		public IQueryable<UndeliveredOrder> GetUndeliveriesForOrders(IUnitOfWork unitOfWork, IList<int> ordersIds)
		{
			var undeliveredOrder =
				from uo in unitOfWork.Session.Query<UndeliveredOrder>()
				join guilty in unitOfWork.Session.Query<GuiltyInUndelivery>() on uo.Id equals guilty.UndeliveredOrder.Id
				join s in unitOfWork.Session.Query<Subdivision>() on guilty.GuiltyDepartment.Id equals s.Id into subdivisions
				from subdivision in subdivisions.DefaultIfEmpty()
				where
				ordersIds.Contains(uo.OldOrder.Id)
				&& (guilty.GuiltySide == GuiltyTypes.Driver || guilty.GuiltyDepartment.SubdivisionType == SubdivisionType.Logistic)
				select uo;

			return undeliveredOrder;
		}

		public IList<OksDailyReportUndeliveredOrderDataNode> GetUndeliveredOrdersForPeriod(IUnitOfWork uow, DateTime startDate, DateTime endDate)
		{
			UndeliveredOrder undeliveredOrderAlias = null;
			Order orderAlias = null;
			Counterparty counterpartyAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;
			Subdivision subdivisionAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			UndeliveredOrderResultComment undeliveredOrderResultCommentAlias = null;
			OksDailyReportUndeliveredOrderDataNode resultAlias = null;

			var resultCommentProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property(nameof(undeliveredOrderResultCommentAlias.Comment)),
						Projections.Constant(" || ")
						);

			var driversSubquery = QueryOver.Of(() => routeListItemAlias)
				.Where(() => routeListItemAlias.Order.Id == orderAlias.Id)
				.Left.JoinQueryOver(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Select(
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String,
							"GROUP_CONCAT(CONCAT(?1, ' ', LEFT(?2,1),'.',LEFT(?3,1)) ORDER BY ?4 DESC SEPARATOR '\n\t↑\n')"), //⬆
						NHibernateUtil.String,
						Projections.Property(() => driverAlias.LastName),
						Projections.Property(() => driverAlias.Name),
						Projections.Property(() => driverAlias.Patronymic),
						Projections.Property(() => routeListItemAlias.Id)
					)
				);

			var resultCommentsSubquery = QueryOver.Of(() => undeliveredOrderResultCommentAlias)
				.Where(r => r.UndeliveredOrder.Id == undeliveredOrderAlias.Id)
				.Select(resultCommentProjection);

			var undeliveredData = uow.Session.QueryOver(() => undeliveredOrderAlias)
				.JoinAlias(() => undeliveredOrderAlias.OldOrder, () => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinEntityAlias(
					() => guiltyInUndeliveryAlias,
					() => guiltyInUndeliveryAlias.UndeliveredOrder.Id == undeliveredOrderAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias)
				.Where(() => orderAlias.DeliveryDate >= startDate.Date && orderAlias.DeliveryDate <= endDate.LatestDayTime())
				.SelectList(list => list
				.Select(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.UndeliveredOrderId)
				.Select(() => undeliveredOrderAlias.NewOrder.Id).WithAlias(() => resultAlias.NewOrderId)
				.Select(() => guiltyInUndeliveryAlias.GuiltySide).WithAlias(() => resultAlias.GuiltySide)
				.Select(() => subdivisionAlias.Id).WithAlias(() => resultAlias.GuiltySubdivisionId)
				.Select(() => subdivisionAlias.ShortName).WithAlias(() => resultAlias.GuiltySubdivisionName)
				.Select(() => undeliveredOrderAlias.UndeliveryStatus).WithAlias(() => resultAlias.UndeliveryStatus)
				.Select(() => undeliveredOrderAlias.OrderTransferType).WithAlias(() => resultAlias.TransferType)
				.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.OldOrderDeliveryDate)
				.Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.ClientName)
				.Select(() => undeliveredOrderAlias.Reason).WithAlias(() => resultAlias.Reason)
				.SelectSubQuery(driversSubquery).WithAlias(() => resultAlias.Drivers)
				.SelectSubQuery(resultCommentsSubquery).WithAlias(() => resultAlias.ResultComments))
				.TransformUsing(Transformers.AliasToBean<OksDailyReportUndeliveredOrderDataNode>())
				.List<OksDailyReportUndeliveredOrderDataNode>();

			return undeliveredData;
		}
	}
}
