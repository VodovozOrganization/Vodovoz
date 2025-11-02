using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Settings.Logistics;
using VodovozInfrastructure.Versions;

namespace Vodovoz.Infrastructure.Persistance.Sale
{
	internal sealed class GeographicGroupRepository : IGeographicGroupRepository
	{
		public GeoGroup GeographicGroupByCoordinates(double? lat, double? lon, IList<District> source)
		{
			GeoGroup gg = null;

			if(lat.HasValue && lon.HasValue)
			{
				var point = new Point(lat.Value, lon.Value);
				gg = source.FirstOrDefault(d => d.DistrictBorder != null && d.DistrictBorder.Contains(point))?
						   .GeographicGroup;
			}

			return gg;
		}

		public IList<GeoGroup> GeographicGroupsWithCoordinates(IUnitOfWork uow, bool isActiveOnly = false)
		{
			GeoGroup geoGroupAlias = null;
			GeoGroupVersion geoGroupVersionAlias = null;

			var query = uow.Session.QueryOver(() => geoGroupAlias)
				.Left.JoinAlias(() => geoGroupAlias.Versions, () => geoGroupVersionAlias)
				.Where(
					Restrictions.Conjunction()
						.Add(Restrictions.IsNotNull(Projections.Property(() => geoGroupVersionAlias.BaseLatitude)))
						.Add(Restrictions.IsNotNull(Projections.Property(() => geoGroupVersionAlias.BaseLongitude)))
				);

			if(isActiveOnly)
			{
				query.Where(() => geoGroupVersionAlias.Status == VersionStatus.Active);
			}

			return query.List();
		}

		public IList<GeoGroupVersion> GetGeographicGroupVersionsOnDate(IUnitOfWork uow, DateTime date)
		{
			GeoGroupVersion geoGroupVersionAlias = null;

			var query = uow.Session.QueryOver(() => geoGroupVersionAlias)
				.Where(() => geoGroupVersionAlias.ActivationDate <= date)
				.Where(() => geoGroupVersionAlias.ClosingDate >= date)
				.Where(
					Restrictions.Conjunction()
						.Add(Restrictions.IsNotNull(Projections.Property(() => geoGroupVersionAlias.BaseLatitude)))
						.Add(Restrictions.IsNotNull(Projections.Property(() => geoGroupVersionAlias.BaseLongitude)))
				);
			return query.List();
		}

		public IList<GeoGroup> GeographicGroupsWithCoordinatesExceptEast(
			IUnitOfWork uow, IGeographicGroupSettings geographicGroupSettings)
		{
			GeoGroup geoGroupAlias = null;
			GeoGroupVersion geoGroupVersionAlias = null;

			var query = uow.Session.QueryOver(() => geoGroupAlias)
				.Left.JoinAlias(() => geoGroupAlias.Versions, () => geoGroupVersionAlias)
				.Where(
					Restrictions.Conjunction()
						.Add(Restrictions.IsNotNull(Projections.Property(() => geoGroupVersionAlias.BaseLatitude)))
						.Add(Restrictions.IsNotNull(Projections.Property(() => geoGroupVersionAlias.BaseLongitude)))
				)
				.Where(gg => gg.Id != geographicGroupSettings.EastGeographicGroupId);
			return query.List();
		}

		public IList<GeoGroup> GeographicGroupsWithoutEast(
			IUnitOfWork uow, IGeographicGroupSettings geographicGroupSettings)
		{
			GeoGroup geoGroupAlias = null;

			var query = uow.Session.QueryOver(() => geoGroupAlias)
				.Where(() => geoGroupAlias.Id != geographicGroupSettings.EastGeographicGroupId);

			return query.List();
		}
	}
}
