using System.Collections.Generic;
using System.Linq;
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

		public static IList<ScheduleRestrictedDistrict> AreasWithGeometry(IUnitOfWork uow)
		{
			return AreaWithGeometryQuery()
							.GetExecutableQueryOver(uow.Session)
							.List();
		}

		/// <summary>
		/// Список складов, которые обслуживают район
		/// </summary>
		/// <returns>Список складо</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="district">Район</param>
		public static IList<Warehouse> GetShippingWarehousesForDistrict(IUnitOfWork uow, ScheduleRestrictedDistrict district)
		{
			var ggId = district.GeographicGroups?.FirstOrDefault().Id;
			if(ggId == null)
				return new List<Warehouse>();
			var subquery = QueryOver.Of<Subdivision>()
									.Where(s => s.GeographicGroup.Id == ggId)
									.Select(s => s.Id)
									;

			var whs = uow.Session.QueryOver<Warehouse>()
						.WithSubquery
						.WhereProperty(w => w.OwningSubdivision.Id).In(subquery)
						.List()
						;

			return whs;
		}
	}
}
