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

namespace Vodovoz.EntityRepositories.Sale
{
	public class ScheduleRestrictionRepository : IScheduleRestrictionRepository
	{
		public QueryOver<SectorVersion> GetSectorVersion(DateTime? activationTime) {
			if(activationTime.HasValue)
				return QueryOver.Of<SectorVersion>()
				.Where(x => x.StartDate <= activationTime.Value && (x.EndDate == null || x.EndDate <= activationTime.Value.Date.AddDays(1) ))
				.And(x => x.Polygon != null);
			return QueryOver.Of<SectorVersion>()
				.Where(x => x.Status == SectorsSetStatus.Active)
				.And(x => x.Polygon != null);
		}
		public IList<SectorVersion> GetSectorVersion(IUnitOfWork uow, DateTime? activationTime)
		{
			return GetSectorVersion(activationTime)
				.GetExecutableQueryOver(uow.Session)
				.List();
		}

		public IEnumerable<OrderCountResultNode> OrdersCountByDistrict(IUnitOfWork uow, DateTime date, int minBottlesInOrder)
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
	}
}