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
		//public static PointLatLng BasePoint = new PointLatLng(Constants.BaseLatitude, Constants.BaseLongitude);

		public static double GetDistance(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return GetDistance(fromDP.GmapPoint, toDP.GmapPoint);
		}

		public static double GetDistance(PointLatLng fromPoint, PointLatLng toPoint)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(fromPoint, toPoint);
		}

		public static double GetDistanceFromBase(GeographicGroup fromBase, DeliveryPoint toDP)
		{
			var basePoint = new PointLatLng((double)fromBase.BaseLatitude.Value, (double)fromBase.BaseLongitude.Value);
			return (int)GetDistance(basePoint, toDP.GmapPoint);
		}

		public static double GetDistanceToBase(DeliveryPoint fromDP, GeographicGroup toBase)
		{
			var basePoint = new PointLatLng((double)toBase.BaseLatitude.Value, (double)toBase.BaseLongitude.Value);
			return (int)GetDistance(fromDP.GmapPoint, basePoint);
		}

		public int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			return (int)(GetDistance(fromDP, toDP) * 1000);
		}

		public int DistanceFromBaseMeter(GeographicGroup fromBase, DeliveryPoint toDP)
		{
			return (int)(GetDistanceFromBase(fromBase, toDP) * 1000);
		}

		public int DistanceToBaseMeter(DeliveryPoint fromDP, GeographicGroup toBase)
		{
			return (int)(GetDistanceToBase(fromDP, toBase) * 1000);
		}
	}
}
