using GMap.NET;
using NetTopologySuite.Geometries;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class DriverPositionExtensions
	{
		public static Coordinate ToCoordinate(this DriverPosition driverPosition) =>
			new Coordinate(driverPosition.Latitude, driverPosition.Longitude);

		public static PointLatLng ToPointLatLng(this DriverPosition driverPosition) =>
			new PointLatLng(driverPosition.Latitude, driverPosition.Longitude);
	}
}
