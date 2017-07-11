using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Logistic
{
	public class CachedDistance
	{

		public virtual long FromGeoHash { get; set; }

		public virtual long ToGeoHash { get; set; }

		public virtual int DistanceMeters { get; set; }

		public virtual int TravelTimeSec { get; set; }

		public virtual DateTime Created { get; set; }

		public CachedDistance()
		{
		}

		public static long GetHash(DeliveryPoint point)
		{
			// A - Latitude; O - Longitude; hash = AA.AAAAOO.OOOO -> (long)AAAAAAOOOOOO
			return (long)(point.Latitude.Value * 10000) * 1000000 + (long)(point.Longitude.Value * 10000);
		}

		public static long GetHash(double latitude, double longitude)
		{
			// A - Latitude; O - Longitude; hash = AA.AAAAOO.OOOO -> (long)AAAAAAOOOOOO
			return (long)(latitude * 10000) * 1000000 + (long)(longitude * 10000);
		}

		public static void GetLatLon(long hash, out double latitude, out double longitude)
		{
			latitude = (double)(hash / 1000000) / 10000;
			longitude = (double)(hash % 1000000) / 10000;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CachedDistance;

			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;

			return this.FromGeoHash == other.FromGeoHash &&
				       this.ToGeoHash == other.ToGeoHash;
		}

		public override int GetHashCode()
		{
			unchecked {
				int hash = GetType().GetHashCode();
				hash = (hash * 31) ^ FromGeoHash.GetHashCode();
				hash = (hash * 31) ^ ToGeoHash.GetHashCode();

				return hash;
			}
		}
	}
}
