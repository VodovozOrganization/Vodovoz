using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repositories.Sale
{
	public static class ScheduleRestrictionRepository
	{
		public static QueryOver<ScheduleRestrictedDistrict> AreaWithGeometryQuery()
		{
			return QueryOver.Of<ScheduleRestrictedDistrict>().Where(x => x.DistrictBorder != null);
		}

		public static IList<ScheduleRestrictedDistrict> AreaWithGeometry(IUnitOfWork uow)
		{
			return AreaWithGeometryQuery()
							.GetExecutableQueryOver(uow.Session)
							.List();
		}

		/// <summary>
		/// Список складов, которые обслужива
		/// </summary>
		/// <returns>The shipping warehouse for district.</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="district">District.</param>
		public static IList<Warehouse> GetShippingWarehouseForDistrict(IUnitOfWork uow, ScheduleRestrictedDistrict district)
		{
			if(district == null)
				return new List<Warehouse>();
			Subdivision subdivisionAlias = null;
			ScheduleRestrictedDistrict districtAlias = null;
			var subquery = QueryOver.Of<Subdivision>()
									.JoinAlias(x => x.ServicingDistricts, () => districtAlias)
									.Where(() => districtAlias.Id == district.Id)
									.Select(s => s.Id)
									;

			var whs = uow.Session.QueryOver<Warehouse>()
						.Left.JoinAlias(w => w.OwningSubdivision, () => subdivisionAlias)
						.WithSubquery
						.WhereProperty(w => w.OwningSubdivision.Id).In(subquery)
						.List()
						;

			return whs;
		}
	}
}
