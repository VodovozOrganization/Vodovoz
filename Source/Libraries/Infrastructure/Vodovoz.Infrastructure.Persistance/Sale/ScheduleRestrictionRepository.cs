using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Sale;

namespace Vodovoz.Infrastructure.Persistance.Sale
{
	internal sealed class ScheduleRestrictionRepository : IScheduleRestrictionRepository
	{
		public QueryOver<District> GetDistrictsWithBorder()
		{
			DistrictsSet districtsSetAlias = null;
			return QueryOver.Of<District>()
				.Left.JoinAlias(x => x.DistrictsSet, () => districtsSetAlias)
				.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active)
				.And(x => x.DistrictBorder != null);
		}

		public IList<District> GetDistrictsWithBorder(IUnitOfWork uow)
		{
			return GetDistrictsWithBorder()
				.GetExecutableQueryOver(uow.Session)
				.List();
		}

		public int GetDistrictsForFastDeliveryCurrentVersionId(IUnitOfWork unitOfWork)
		{
			DistrictsSet districtsSetAlias = null;
			District districtAlias = null;
			TariffZone tariffZoneAlias = null;

			return unitOfWork.Session.QueryOver(() => districtAlias)
				.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.JoinAlias(() => districtAlias.TariffZone, () => tariffZoneAlias)
				.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active)
				.And(() => districtAlias.DistrictBorder != null)
				.And(() => tariffZoneAlias.IsFastDeliveryAvailable)
				.Select(Projections.Property(() => districtsSetAlias.Id))
				.Take(1)
				.SingleOrDefault<int>();
		}

		/// <summary>
		/// Ввиду недоработок версий районов отображает некорректно
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public int GetDistrictsForFastDeliveryHistoryVersionId(IUnitOfWork unitOfWork, DateTime dateTime)
		{
			DistrictsSet districtsSetAlias = null;
			District districtAlias = null;
			TariffZone tariffZoneAlias = null;

			return unitOfWork.Session.QueryOver(() => districtAlias)
				.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.JoinAlias(() => districtAlias.TariffZone, () => tariffZoneAlias)
				.And(() => districtAlias.DistrictBorder != null)
				.And(() => tariffZoneAlias.IsFastDeliveryAvailable)
				.And(Restrictions.Or(
						Restrictions.Le(Projections.Property(() => districtsSetAlias.DateActivated), dateTime),
						Restrictions.Le(Projections.Property(() => districtsSetAlias.DateCreated), dateTime)))
				.And(Restrictions.Or(
						Restrictions.And(
							Restrictions.IsNull(Projections.Property(() => districtsSetAlias.DateClosed)),
							Restrictions.Eq(Projections.Property(() => districtsSetAlias.Status), DistrictsSetStatus.Active)),
						Restrictions.Ge(Projections.Property(() => districtsSetAlias.DateClosed), dateTime)))
				.Select(Projections.Property(() => districtsSetAlias.Id))
				.OrderBy(Projections.Property(() => districtsSetAlias.DateClosed)).Desc
				.Take(1)
				.SingleOrDefault<int>();
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

		/// <summary>
		/// Ввиду недоработок версий районов отображает некорректно
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public IList<District> GetDistrictsWithBorderForFastDeliveryAtDateTime(IUnitOfWork uow, DateTime dateTime)
		{
			DistrictsSet districtsSetAlias = null;
			District districtAlias = null;
			TariffZone tariffZoneAlias = null;

			var historyVersionId = GetDistrictsForFastDeliveryHistoryVersionId(uow, dateTime);

			return uow.Session.QueryOver(() => districtAlias)
				.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.JoinAlias(() => districtAlias.TariffZone, () => tariffZoneAlias)
				.And(() => districtAlias.DistrictBorder != null)
				.And(() => tariffZoneAlias.IsFastDeliveryAvailable)
				.And(Restrictions.Or(
						Restrictions.Le(Projections.Property(() => districtsSetAlias.DateActivated), dateTime),
						Restrictions.Le(Projections.Property(() => districtsSetAlias.DateCreated), dateTime)))
				.And(Restrictions.Or(
					Restrictions.And(
						Restrictions.IsNull(Projections.Property(() => districtsSetAlias.DateClosed)),
						Restrictions.Eq(Projections.Property(() => districtsSetAlias.Status), DistrictsSetStatus.Active)),
					Restrictions.Ge(Projections.Property(() => districtsSetAlias.DateClosed), dateTime)))
				.And(() => districtsSetAlias.Id == historyVersionId)
				.Select(Projections.Entity(() => districtAlias))
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
				.Where(x => x.Category == NomenclatureCategory.water && !x.IsDisposableTare)
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
