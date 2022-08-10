using System;
using GMap.NET;
using GMap.NET.MapProviders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Класс предназначен для расчета расстояний между точками.
	/// Расчет происходит напрямую без учета дорожной сети.
	/// </summary>
	public class DistanceCalculator : IDistanceCalculator
	{
		public static double GetDistance(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return GetDistance(fromDP.GmapPoint, toDP.GmapPoint);
		}

		public static double GetDistance(PointLatLng fromPoint, PointLatLng toPoint)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(fromPoint, toPoint);
		}

		public static double GetDistanceFromBase(GeoGroup fromBase, DeliveryPoint toDP)
		{
			var geoGroupVersion = GetGeoGroupVersion(fromBase);
			var basePoint = new PointLatLng((double)geoGroupVersion.BaseLatitude.Value, (double)geoGroupVersion.BaseLongitude.Value);
			return (int)GetDistance(basePoint, toDP.GmapPoint);
		}

		public static double GetDistanceToBase(DeliveryPoint fromDP, GeoGroup toBase)
		{
			var geoGroupVersion = GetGeoGroupVersion(toBase);
			var basePoint = new PointLatLng((double)geoGroupVersion.BaseLatitude.Value, (double)geoGroupVersion.BaseLongitude.Value);
			return (int)GetDistance(fromDP.GmapPoint, basePoint);
		}

		private static GeoGroupVersion GetGeoGroupVersion(GeoGroup geoGroup)
		{
			var geoGroupVersion = geoGroup.GetActualVersionOrNull();
			if(geoGroupVersion == null)
			{
				throw new InvalidOperationException($"Не установлена активная версия данных в части города {geoGroup.Name}");
			}
			return geoGroupVersion;
		}

		public static PointLatLng FindPointByDistanceAndRadians(PointLatLng startPoint, double initialRadians, double distanceKilometers)
		{
			const double radiusEarthKilometers = 6371.01d;
			var distRatio = distanceKilometers / radiusEarthKilometers;
			var distRatioSine = Math.Sin(distRatio);
			var distRatioCosine = Math.Cos(distRatio);

			var startLatRad = PureProjection.DegreesToRadians(startPoint.Lat);
			var startLonRad = PureProjection.DegreesToRadians(startPoint.Lng);

			var startLatCos = Math.Cos(startLatRad);
			var startLatSin = Math.Sin(startLatRad);

			var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialRadians)));
			var endLonRads = startLonRad + Math.Atan2(Math.Sin(initialRadians) * distRatioSine * startLatCos, distRatioCosine - startLatSin * Math.Sin(endLatRads));

			return new PointLatLng(PureProjection.RadiansToDegrees(endLatRads), PureProjection.RadiansToDegrees(endLonRads));
		}

		public int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return (int)(GetDistance(fromDP, toDP) * 1000);
		}

		public int DistanceFromBaseMeter(GeoGroup fromBase, DeliveryPoint toDP)
		{
			return (int)(GetDistanceFromBase(fromBase, toDP) * 1000);
		}

		public int DistanceToBaseMeter(DeliveryPoint fromDP, GeoGroup toBase)
		{
			return (int)(GetDistanceToBase(fromDP, toBase) * 1000);
		}
	}
}
