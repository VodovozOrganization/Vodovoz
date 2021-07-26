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
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Repositories.Sale
{
	public static class ScheduleRestrictionRepository
	{
		public static QueryOver<SectorVersion> GetSectorVersion()
		{
			return QueryOver.Of<SectorVersion>()
				.Where(x => x.Status == SectorsSetStatus.Active)
				.And(x => x.Polygon != null);
		}

		public static IList<SectorVersion> GetSectorVersion(IUnitOfWork uow)
		{
			return GetSectorVersion()
				.GetExecutableQueryOver(uow.Session)
				.List();
		}

		public static IEnumerable<OrderCountResultNode> OrdersCountByDistrict(IUnitOfWork uow, DateTime date, int minBottlesInOrder)
		{
			OrderCountResultNode resultAlias = null;
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			DeliveryPointSectorVersion deliveryPointSectorVersion = null;

			var districtSubquery = QueryOver.Of<Sector>()
				.Where(
					Restrictions.Eq(
						Projections.SqlFunction(
							new SQLFunctionTemplate(
								NHibernateUtil.Boolean,
								"ST_WITHIN(PointFromText(CONCAT('POINT(', ?1 ,' ', ?2,')')), ?3)"
							),
							NHibernateUtil.Boolean,
							Projections.Property(() => deliveryPointSectorVersion.Latitude),
							Projections.Property(() => deliveryPointSectorVersion.Longitude),
							Projections.Property<SectorVersion>(x => x.Polygon)
						),
						true
					)
				)
				.Select(x => x.Id)
				.Take(1);

			return uow.Session.QueryOver(() => orderAlias)
				.Where(x => x.DeliveryDate == date)
				.Where(x => x.OrderStatus == OrderStatus.Accepted || x.OrderStatus == OrderStatus.InTravelList)
				.JoinQueryOver(x => x.OrderItems, () => orderItemsAlias)
				.JoinQueryOver(x => x.Nomenclature)
				.Where(x => x.Category == Domain.Goods.NomenclatureCategory.water && !x.IsDisposableTare)
				.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointSectorVersion)
				.SelectList(list => list.SelectGroup(x => x.Id).WithAlias(() => resultAlias.OrderId)
					.SelectSum(() => (int)orderItemsAlias.Count).WithAlias(() => resultAlias.WaterCount)
					.SelectSubQuery(districtSubquery).WithAlias(() => resultAlias.DistrictId)
				)
				.Where(Restrictions.Gt(
					Projections.Sum(
						Projections.Property(() => orderItemsAlias.Count)), 12))
				.TransformUsing(Transformers.AliasToBean<OrderCountResultNode>())
				.List<OrderCountResultNode>();
		}

		public class OrderCountResultNode
		{
			public int OrderId { get; set; }
			public int WaterCount { get; set; }
			public int DistrictId { get; set; }
		}
	}
}