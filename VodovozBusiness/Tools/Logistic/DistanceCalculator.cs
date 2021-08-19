using System;
using GMap.NET;
using GMap.NET.MapProviders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Класс предназначен для расчета расстояний между точками.
	/// Расчет происходит напрямую без учета дорожной сети.
	/// </summary>
	public class DistanceCalculator : IDistanceCalculator
	{
		//public static PointLatLng BasePoint = new PointLatLng(Constants.BaseLatitude, Constants.BaseLongitude);

		public static double GetDistance(DeliveryPoint fromDP, DeliveryPoint toDP, DateTime? activationTimeOneVersion, DateTime? activationTimeTwoVersion)
		{
			return GetDistance(fromDP.GetActiveVersion(activationTimeOneVersion).GmapPoint, toDP.GetActiveVersion(activationTimeTwoVersion).GmapPoint);
		}

		public static double GetDistance(PointLatLng fromPoint, PointLatLng toPoint)
		{
			return GMapProviders.EmptyProvider.Projection.GetDistance(fromPoint, toPoint);
		}

		public static double GetDistanceFromBase(GeographicGroup fromBase, DeliveryPoint toDP, DateTime? activationTime)
		{
			var basePoint = new PointLatLng((double)fromBase.BaseLatitude.Value, (double)fromBase.BaseLongitude.Value);
			return (int)GetDistance(basePoint, toDP.GetActiveVersion(activationTime).GmapPoint);
		}

		public static double GetDistanceToBase(DeliveryPoint fromDP, GeographicGroup toBase, DateTime? activationTime)
		{
			var basePoint = new PointLatLng((double)toBase.BaseLatitude.Value, (double)toBase.BaseLongitude.Value);
			return (int)GetDistance(fromDP.GetActiveVersion().GmapPoint, basePoint);
		}

		public int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP, DateTime? activationTimeOneVersion, DateTime? activationTimeTwoVersion)
		{
			return (int)(GetDistance(fromDP, toDP, activationTimeOneVersion, activationTimeTwoVersion) * 1000);
		}

		public int DistanceFromBaseMeter(GeographicGroup fromBase, DeliveryPoint toDP, DateTime? activationTime)
		{
			return (int)(GetDistanceFromBase(fromBase, toDP, activationTime) * 1000);
		}

		public int DistanceToBaseMeter(DeliveryPoint fromDP, GeographicGroup toBase, DateTime? activationTime)
		{
			return (int)(GetDistanceToBase(fromDP, toBase, activationTime) * 1000);
		}
	}
}
