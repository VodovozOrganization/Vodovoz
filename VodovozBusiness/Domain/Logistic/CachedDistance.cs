using System;
using System.Globalization;
using GMap.NET;
using QSOsm;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	public class CachedDistance
	{

		public virtual long FromGeoHash { get; set; }

		public virtual long ToGeoHash { get; set; }

		public virtual int DistanceMeters { get; set; }

		public virtual int TravelTimeSec { get; set; }

		public virtual string PolylineGeometry { get; set; }

		public virtual DateTime Created { get; set; }

		public CachedDistance() { }

		#region Static

		public static long GetHash(DeliveryPoint point) => GetHash((double)point.Latitude.Value, (double)point.Longitude.Value);

		public static long GetHash(GeographicGroup point) => GetHash((double)point.BaseLatitude.Value, (double)point.BaseLongitude.Value);

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

		public static PointLatLng GetPointLatLng(long hash)
		{
			GetLatLon(hash, out double lat, out double lon);
			return new PointLatLng(lat, lon);
		}

		public static PointOnEarth GetPointOnEarth(long hash)
		{
			GetLatLon(hash, out double lat, out double lon);
			return new PointOnEarth(lat, lon);
		}

		public static string GetText(long hash)
		{
			GetLatLon(hash, out double latitude, out double longitude);
			return String.Format(CultureInfo.InvariantCulture, "{0},{1}", latitude, longitude);
		}

		public static string GetTextLonLat(long hash)
		{
			GetLatLon(hash, out double latitude, out double longitude);
			return String.Format(CultureInfo.InvariantCulture, "{0},{1}", longitude, latitude);
		}

		//public static long BaseHash => GetHash(Constants.BaseLatitude, Constants.BaseLongitude);

		#endregion

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
