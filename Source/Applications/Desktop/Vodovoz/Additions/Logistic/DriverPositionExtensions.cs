using GMap.NET;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Additions.Logistic
{
	public static class DriverPositionExtensions
	{
		public static PointLatLng ToPointLatLng(this DriverPosition driverPosition) => new PointLatLng(driverPosition.Latitude, driverPosition.Longitude);
	}
}
