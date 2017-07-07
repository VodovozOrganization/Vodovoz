using GMap.NET;
using GMap.NET.MapProviders;
using Vodovoz.Domain.Client;

namespace Vodovoz.Tools.Logistic
{
	public static class DistanceCalculator
	{
		static PointLatLng BasePoint = new PointLatLng(Constants.BaseLatitude, Constants.BaseLongitude);

		public static double GetDistance(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(fromDP.GmapPoint, toDP.GmapPoint);
		}

		public static double GetDistanceFromBase(DeliveryPoint toDP)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(BasePoint, toDP.GmapPoint);
		}

		public static double GetDistanceToBase(DeliveryPoint fromDP)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(fromDP.GmapPoint, BasePoint);
		}

	}
}
