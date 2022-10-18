using FluentNHibernate.Mapping;
using Vodovoz.Domain.Geocoder;

namespace Vodovoz.HibernateMapping.Geocoder
{
	public class GeocoderCoordinatesCacheMap : ClassMap<GeocoderCoordinatesCache>
	{
		public GeocoderCoordinatesCacheMap()
		{
			Table("geocoder_coordinates_cache");

			CompositeId()
				.KeyProperty(x => x.Latitude, "latitude")
				.KeyProperty(x => x.Longitude, "longitude");

			Map(x => x.Address).Column("address");
		}
	}
}
