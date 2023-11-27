using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using GMap.NET.MapProviders;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using NHibernate.Util;
using QS.Utilities.Spatial;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Extensions;

namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Класс предназначен для расчета расстояний между точками.
	/// Расчет происходит напрямую без учета дорожной сети.
	/// </summary>
	public class DistanceCalculator : IDistanceCalculator, IFastDeliveryDistanceChecker
	{
		const double _radiusEarthKilometers = 6371.01d;

		public static double GetDistance(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return GetDistance(fromDP.GmapPoint, toDP.GmapPoint);
		}

		public static double GetDistance(PointLatLng fromPoint, PointLatLng toPoint)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(fromPoint, toPoint);
		}

		public static double GetDistance(Coordinate firstCoordinate, Coordinate secondCoordinate)
		{
			return firstCoordinate.Distance(secondCoordinate);
		}

		public static double GetDistanceFromBase(GeoGroupVersion fromBase, DeliveryPoint toDP)
		{
			var basePoint = new PointLatLng((double)fromBase.BaseLatitude.Value, (double)fromBase.BaseLongitude.Value);
			return (int)GetDistance(basePoint, toDP.GmapPoint);
		}

		public static double GetDistanceToBase(DeliveryPoint fromDP, GeoGroupVersion toBase)
		{
			var basePoint = new PointLatLng((double)toBase.BaseLatitude.Value, (double)toBase.BaseLongitude.Value);
			return (int)GetDistance(fromDP.GmapPoint, basePoint);
		}

		public static PointLatLng FindPointByDistanceAndRadians(PointLatLng startPoint, double initialRadians, double distanceKilometers)
		{
			var distRatio = distanceKilometers / _radiusEarthKilometers;
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

		public static Coordinate FindPointByDistanceAndRadians(Coordinate startPoint, double initialRadians, double distanceKilometers)
		{
			var multiplierDeg2Rad = Math.PI / 180;
			var multiplierRad2Deg = 180 / Math.PI;

			var distRatio = distanceKilometers / _radiusEarthKilometers;
			var distRatioSine = Math.Sin(distRatio);
			var distRatioCosine = Math.Cos(distRatio);

			var startLatRad = multiplierDeg2Rad * startPoint.X;
			var startLonRad = multiplierDeg2Rad * startPoint.Y;

			var startLatCos = Math.Cos(startLatRad);
			var startLatSin = Math.Sin(startLatRad);

			var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialRadians)));
			var endLonRads = startLonRad + Math.Atan2(Math.Sin(initialRadians) * distRatioSine * startLatCos, distRatioCosine - startLatSin * Math.Sin(endLatRads));

			return new Coordinate(multiplierRad2Deg * endLatRads, multiplierRad2Deg * endLonRads);
		}

		public static double CalculateCoveragePercent(ICollection<Geometry> districtsBorders, ICollection<Coordinate> driversCoordinates, double fastDeliveryRadius)
		{
			var geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);

			if(districtsBorders.Any())
			{
				Geometry allDistricts = CascadedPolygonUnion.Union(districtsBorders);

				var totalDistrictsArea = allDistricts.Area;

				Geometry allRadiuces =
					CascadedPolygonUnion.Union(
						driversCoordinates.Select(x => CreateCircle(x, fastDeliveryRadius))
					.ToList());

				var difference = allDistricts.Difference(allRadiuces).Area;

				return totalDistrictsArea != 0 ? (totalDistrictsArea - difference) / totalDistrictsArea : 0;

			}
			return 0;
		}

		public static double CalculateCoveragePercent(ICollection<Geometry> districtsBorders, ICollection<DriverPositionWithFastDeliveryRadius> drivers)
		{
			var geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);

			if(districtsBorders.Any())
			{
				Geometry allDistricts = CascadedPolygonUnion.Union(districtsBorders);

				var totalDistrictsArea = allDistricts.Area;

				Geometry allRadiuces =
					CascadedPolygonUnion.Union(
						drivers.Select(x => CreateCircle(x.ToCoordinate(), x.FastDeliveryRadius))
					.ToList());

				var difference = allDistricts.Difference(allRadiuces).Area;

				return totalDistrictsArea != 0 ? (totalDistrictsArea - difference) / totalDistrictsArea : 0;

			}
			return 0;
		}

		private static Geometry CreateCircle(Coordinate center, double radius)
		{
			var twoPi = 2 * Math.PI;

			var segmentsPointsCount = 36;

			var geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);

			var perimetralRingPoints = new List<Coordinate>();

			for(double radian = 0; radian < twoPi; radian += twoPi / segmentsPointsCount)
			{
				perimetralRingPoints.Add(FindPointByDistanceAndRadians(center, radian, radius));
			}
			perimetralRingPoints.Add(perimetralRingPoints.First());

			Polygon polyCircle = geometryFactory.CreatePolygon(perimetralRingPoints.ToArray());

			return polyCircle;
		}

		public int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return (int)(GetDistance(fromDP, toDP) * 1000);
		}

		public int DistanceFromBaseMeter(GeoGroupVersion fromBase, DeliveryPoint toDP)
		{
			return (int)(GetDistanceFromBase(fromBase, toDP) * 1000);
		}

		public int DistanceToBaseMeter(DeliveryPoint fromDP, GeoGroupVersion toBase)
		{
			return (int)(GetDistanceToBase(fromDP, toBase) * 1000);
		}

		public bool DeliveryPointInFastDeliveryRadius(DeliveryPoint deliveryPoint, DriverPosition driverPosition, decimal radius)
		{
			var distance = DistanceHelper.GetDistanceKm(driverPosition.Latitude, driverPosition.Longitude, Convert.ToDouble(deliveryPoint.Latitude), Convert.ToDouble(deliveryPoint.Longitude));

			return distance <= Convert.ToDouble(radius);
		}
	}
}
