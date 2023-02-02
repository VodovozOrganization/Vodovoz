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

namespace Vodovoz.EntityRepositories.Sale
{
	public class ScheduleRestrictionRepository : IScheduleRestrictionRepository
	{
		public QueryOver<District> GetDistrictsWithBorder()
		{
			DistrictsSet districtsSetAlias = null;
			return QueryOver.Of<District>()
				.Left.JoinAlias(x => x.DistrictsSet, () => districtsSetAlias)
				.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active)
				.And(x => x.DistrictBorder != null);
		}

		public IList<District> GetDistrictsWithBorderForFastDelivery(IUnitOfWork uow)
		{
			DistrictsSet districtsSetAlias = null;
			District districtAlias = null;
			TariffZone tariffZoneAlias = null;

			return uow.Session.QueryOver(() => districtAlias)
				.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.JoinAlias(() => districtAlias.TariffZone, () => tariffZoneAlias)
				.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active)
				.And(() => districtAlias.DistrictBorder != null)
				.And(() => tariffZoneAlias.IsFastDeliveryAvailable)
				.Select(Projections.Entity(() => districtAlias))
				.List();
		}

		public IList<District> GetDistrictsWithBorderForFastDeliveryAtDateTime(IUnitOfWork uow, DateTime dateTime)
		{
			DistrictsSet districtsSetAlias = null;
			District districtAlias = null;
			TariffZone tariffZoneAlias = null;

			return uow.Session.QueryOver(() => districtAlias)
				.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.JoinAlias(() => districtAlias.TariffZone, () => tariffZoneAlias)
				.And(() => districtAlias.DistrictBorder != null)
				.And(() => tariffZoneAlias.IsFastDeliveryAvailable)
				.And(Restrictions.Le(Projections.Property(() => districtsSetAlias.DateActivated), dateTime))
				.And(Restrictions.Or(
					Restrictions.And(
						Restrictions.IsNull(Projections.Property(() => districtsSetAlias.DateClosed)),
						Restrictions.Eq(Projections.Property(() => districtsSetAlias.Status), DistrictsSetStatus.Active)),
					Restrictions.Ge(Projections.Property(() => districtsSetAlias.DateClosed), dateTime)))
				.Select(Projections.Entity(() => districtAlias))
				.List();
		}

		public IList<District> GetDistrictsWithBorder(IUnitOfWork uow)
		{
			return GetDistrictsWithBorder()
				.GetExecutableQueryOver(uow.Session)
				.List();
		}

		public IEnumerable<OrderCountResultNode> OrdersCountByDistrict(IUnitOfWork uow, DateTime date, int minBottlesInOrder)
		{
			OrderCountResultNode resultAlias = null;
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var districtSubquery = QueryOver.Of<District>()
				.Where(
					Restrictions.Eq(
						Projections.SqlFunction(
							new SQLFunctionTemplate(
								NHibernateUtil.Boolean,
								"ST_WITHIN(PointFromText(CONCAT('POINT(', ?1 ,' ', ?2,')')), ?3)"
							),
							NHibernateUtil.Boolean,
							Projections.Property(() => deliveryPointAlias.Latitude),
							Projections.Property(() => deliveryPointAlias.Longitude),
							Projections.Property<District>(x => x.DistrictBorder)
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
				.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.SelectList(list => list.SelectGroup(x => x.Id).WithAlias(() => resultAlias.OrderId)
					.SelectSum(() => orderItemsAlias.Count).WithAlias(() => resultAlias.WaterCount)
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
