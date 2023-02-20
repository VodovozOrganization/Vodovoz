using NetTopologySuite.Geometries;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Extensions
{
	public static class DriverPositionExtensions
	{
		public static Coordinate ToCoordinate(this DriverPosition driverPosition) => new Coordinate(driverPosition.Latitude, driverPosition.Longitude);
	}
}
