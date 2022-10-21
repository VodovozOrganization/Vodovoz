using System;

namespace Vodovoz.Services
{
	public interface IDeliveryRulesParametersProvider
	{
		bool IsStoppedOnlineDeliveriesToday { get; }
		void UpdateOnlineDeliveriesTodayParameter(string value);

		int BottlesCountForFlyer { get; }
		void UpdateBottlesCountForFlyerParameter(string value);

		bool AdditionalLoadingFlyerAdditionEnabled { get; }
		void UpdateAdditionalLoadingFlyerAdditionEnabledParameter(string value);

		#region FastDelivery

		int FastDeliveryScheduleId { get; }
		TimeSpan MaxTimeOffsetForLatestTrackPoint { get; }
		TimeSpan MaxTimeForFastDelivery { get; }
		TimeSpan MinTimeForNewFastDeliveryOrder { get; }
		TimeSpan DriverUnloadTime { get; }
		int DriverGoodWeightLiftPerHandInKg { get; }
		int MaxFastOrdersPerSpecificTime { get; }
		TimeSpan SpecificTimeForMaxFastOrdersCount { get; }
		double MaxDistanceToLatestTrackPointKm { get; }
		void UpdateFastDeliveryMaxDistanceParameter(string value);

		#endregion
	}
}
