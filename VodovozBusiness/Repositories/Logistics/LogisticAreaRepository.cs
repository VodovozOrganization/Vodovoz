using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Repository.Logistics
{
	public static class LogisticAreaRepository
	{
		public static QueryOver<LogisticsArea> ActiveAreaQuery()
		{
			return QueryOver.Of<LogisticsArea>();
		}

		public static QueryOver<LogisticsArea> AreaWithGeometryQuery()
		{
			return QueryOver.Of<LogisticsArea>().Where(x => x.Geometry != null);
		}

		public static IList<LogisticsArea> AreaWithGeometry(IUnitOfWork uow)
		{
			return AreaWithGeometryQuery()
				            .GetExecutableQueryOver(uow.Session)
				            .List();
		}

		public static IList<OrderCountResult> OrdersCountByArea(IUnitOfWork uow, DateTime date, int minBottlesInOrder)
		{
			OrderCountResult resultAlias = null;
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			LogisticsArea areaAlias = null;

			var districtSubquery = QueryOver.Of<LogisticsArea>()
			                                .Where( Restrictions.Eq(
				                                Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Boolean, "ST_WITHIN(PointFromText(CONCAT('POINT(', ?1 ,' ', ?2,')')), ?3)"),
				                                                        NHibernateUtil.Boolean,
																		Projections.Property(() => deliveryPointAlias.Latitude),
																		Projections.Property(() => deliveryPointAlias.Longitude),
				                                                        Projections.Property<LogisticsArea>(x => x.Geometry)
				                                                       ), true)
			                                      )
											.Select(x => x.Id)
											.Take(1);

			return uow.Session.QueryOver<Domain.Orders.Order>(() => orderAlias)
						  .Where(x => x.DeliveryDate == date)
						  .Where(x => x.OrderStatus == Domain.Orders.OrderStatus.Accepted || x.OrderStatus == Domain.Orders.OrderStatus.InTravelList)
						  .JoinQueryOver(x => x.OrderItems, () => orderItemsAlias)
						  .JoinQueryOver(x => x.Nomenclature)
						  .Where(x => x.Category == Domain.Goods.NomenclatureCategory.water)
				                                .JoinAlias(()=> orderAlias.DeliveryPoint, ()=> deliveryPointAlias)
			                .SelectList(list => list.SelectGroup(x => x.Id).WithAlias(() => resultAlias.OrderId)
			                            .SelectSum(() => orderItemsAlias.Count).WithAlias(() => resultAlias.WaterCount)
				                        .SelectSubQuery(districtSubquery).WithAlias(() => resultAlias.DistrictId)
				                       )
							.Where(Restrictions.Gt(
				                Projections.Sum(
								Projections.Property(() => orderItemsAlias.Count)), 12))
							.TransformUsing(Transformers.AliasToBean<OrderCountResult>())
							.List<OrderCountResult>();
		}

		public class OrderCountResult
		{
			public int OrderId;
			public int WaterCount;
			public int DistrictId;
		}
	}
}
