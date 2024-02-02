using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using GMap.NET.MapProviders;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using Vodovoz.Core.Domain.Extensions;

namespace Vodovoz.Core.Domain
{
	/// <summary>
	/// Класс предназначен для расчета расстояний между точками.
	/// Расчет происходит напрямую без учета дорожной сети.
	/// </summary>
	public class DistanceCalculator
	{
		const double _radiusEarthKilometers = 6371.01d;

		public static double GetDistanceMeters(PointLatLng fromPoint, PointLatLng toPoint)
		{
			return GetDistance(fromPoint, toPoint) * 1000;
		}
		
		public static double GetDistanceKilometers(PointLatLng fromPoint, PointLatLng toPoint)
		{
			return GetDistance(fromPoint, toPoint);
		}

		public static double GetDistance(Coordinate firstCoordinate, Coordinate secondCoordinate)
		{
			return firstCoordinate.Distance(secondCoordinate);
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

		public static double CalculateCoveragePercent(
			ICollection<Geometry> districtsBorders,
			ICollection<Coordinate> driversCoordinates,
			double fastDeliveryRadius)
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

		public static double CalculateCoveragePercent(
			ICollection<Geometry> districtsBorders,
			ICollection<DriverPositionWithFastDeliveryRadius> drivers)
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
		
		private static double GetDistance(PointLatLng fromPoint, PointLatLng toPoint)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(fromPoint, toPoint);
		}
	}
}
