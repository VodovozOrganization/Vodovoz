using FluentNHibernate.Mapping;
using Vodovoz.Domain.Geocoder;

namespace Vodovoz.HibernateMapping.Geocoder
{
	public class GeocoderAddressCacheMap : ClassMap<GeocoderAddressCache>
	{
		public GeocoderAddressCacheMap()
		{
			Table("geocoder_address_cache");

			Id(x => x.Address).Column("address").GeneratedBy.Assigned();

			Map(x => x.Latitude).Column("latitude");
			Map(x => x.Longitude).Column("longitude");
		}
	}
}
