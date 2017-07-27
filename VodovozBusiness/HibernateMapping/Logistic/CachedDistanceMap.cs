using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class CachedDistanceMap : ClassMap<CachedDistance>
	{
		public CachedDistanceMap()
		{
			Table("distance_cache");

			CompositeId()
				.KeyProperty(x => x.FromGeoHash, "from_geo")
				.KeyProperty(x => x.ToGeoHash, "to_geo");

			Map(x => x.DistanceMeters).Column("distance");
			Map(x => x.TravelTimeSec).Column("travel_time");
			Map(x => x.Created).Column("created");
			Map(x => x.PolylineGeometry).Column("polyline_geometry").LazyLoad();
		}
	}
}
