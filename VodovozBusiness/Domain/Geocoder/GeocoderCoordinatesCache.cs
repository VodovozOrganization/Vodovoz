namespace Vodovoz.Domain.Geocoder
{
	public class GeocoderCoordinatesCache : AddressCacheBase
	{
		public override bool Equals(object obj)
		{
			return obj is GeocoderCoordinatesCache cache &&
				   Latitude == cache.Latitude &&
				   Longitude == cache.Longitude;
		}

		public override int GetHashCode()
		{
			int hashCode = -1416534245;
			hashCode = hashCode * -1521134295 + Latitude.GetHashCode();
			hashCode = hashCode * -1521134295 + Longitude.GetHashCode();
			return hashCode;
		}
	}
}
