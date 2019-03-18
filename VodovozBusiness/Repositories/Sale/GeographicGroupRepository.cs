using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Repositories.Sale
{
	public static class GeographicGroupRepository
	{
		public static GeographicGroup GeographicGroupByCoordinates(double? lat, double? lon, IUnitOfWork uow)
		{
			return GeographicGroupByCoordinates(lat, lon, ScheduleRestrictionRepository.AreasWithGeometry(uow));
		}

		public static GeographicGroup GeographicGroupByCoordinates(decimal? lat, decimal? lon, IList<ScheduleRestrictedDistrict> source)
		{
			return GeographicGroupByCoordinates(lat, lon, source);
		}

		public static GeographicGroup GeographicGroupByCoordinates(decimal? lat, decimal? lon, IUnitOfWork uow)
		{
			return GeographicGroupByCoordinates(lat, lon, uow);
		}

		public static GeographicGroup GeographicGroupByCoordinates(double? lat, double? lon, IList<ScheduleRestrictedDistrict> source)
		{
			GeographicGroup gg = null;
			if(lat.HasValue && lon.HasValue) {
				var point = new Point(lat.Value, lon.Value);
				gg = source.FirstOrDefault(d => d.DistrictBorder != null && d.DistrictBorder.Contains(point))
						   .GeographicGroups
						   .FirstOrDefault();
			}
			return gg;
		}
		
		public static QueryOver<GeographicGroup> GeographicGroupsWithCoordinatesQuery()
		{
			return QueryOver.Of<GeographicGroup>().Where(x => x.BaseLatitude != null && x.BaseLongitude != null);
		}

		public static IList<GeographicGroup> GeographicGroupsWithCoordinates(IUnitOfWork uow)
		{
			return GeographicGroupsWithCoordinatesQuery()
							.GetExecutableQueryOver(uow.Session)
							.List();
		}
	}
}