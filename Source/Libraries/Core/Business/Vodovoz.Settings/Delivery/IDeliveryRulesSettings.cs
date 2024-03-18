using System;

namespace Vodovoz.Settings.Delivery
{
	public interface IDeliveryRulesSettings
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
		int FlyerForNewCounterpartyBottlesCount { get; }
		bool FlyerForNewCounterpartyEnabled { get; }
		double CarsMonitoringResfreshInSeconds { get; }

		void UpdateFlyerForNewCounterpartyBottlesCountParameter(string value);
		void UpdateFlyerForNewCounterpartyEnabledParameter(string value);
		void UpdateMaxFastOrdersPerSpecificTimeParameter(string toString);


		#endregion
	}
}
