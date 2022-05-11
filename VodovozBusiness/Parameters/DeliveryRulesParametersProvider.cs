using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class DeliveryRulesParametersProvider : IDeliveryRulesParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private const string _onlineDeliveriesTodayParameter = "is_stopped_online_deliveries_today";
		private const string _bottlesCountForFlyerParameter = "bottles_count_for_flyer";
		private const string _additionalLoadingFlyerAdditionEnabledParameter = "additional_loading_flyer_addition_enabled";
		private const string _maxDistanceToLatestTrackPointKm = "fast_delivery_max_distance_km";

		public DeliveryRulesParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public bool IsStoppedOnlineDeliveriesToday => _parametersProvider.GetBoolValue(_onlineDeliveriesTodayParameter);
		public int BottlesCountForFlyer => _parametersProvider.GetValue<int>(_bottlesCountForFlyerParameter);
		public bool AdditionalLoadingFlyerAdditionEnabled =>
			_parametersProvider.GetValue<bool>(_additionalLoadingFlyerAdditionEnabledParameter);

		public void UpdateOnlineDeliveriesTodayParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_onlineDeliveriesTodayParameter, value);

		public void UpdateBottlesCountForFlyerParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_bottlesCountForFlyerParameter, value);

		public void UpdateAdditionalLoadingFlyerAdditionEnabledParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_additionalLoadingFlyerAdditionEnabledParameter, value);

		#region FastDelivery

		public double MaxDistanceToLatestTrackPointKm => _parametersProvider.GetValue<double>(_maxDistanceToLatestTrackPointKm);
		public int DriverGoodWeightLiftPerHandInKg => _parametersProvider.GetValue<int>("fast_delivery_driver_weight_lift_kg");
		public int MaxFastOrdersPerSpecificTime => _parametersProvider.GetValue<int>("fast_delivery_max_orders_per_time");
		public int FastDeliveryScheduleId => _parametersProvider.GetValue<int>("fast_delivery_schedule_id");
		public TimeSpan MaxTimeOffsetForLatestTrackPoint => _parametersProvider.GetValue<TimeSpan>("fast_delivery_time_offset");
		public TimeSpan MaxTimeForFastDelivery => _parametersProvider.GetValue<TimeSpan>("fast_delivery_time");
		public TimeSpan MinTimeForNewFastDeliveryOrder => _parametersProvider.GetValue<TimeSpan>("fast_delivery_min_new_order_time");
		public TimeSpan DriverUnloadTime => _parametersProvider.GetValue<TimeSpan>("fast_delivery_driver_unload_time");
		public TimeSpan SpecificTimeForMaxFastOrdersCount => _parametersProvider.GetValue<TimeSpan>("fast_delivery_time_for_max_orders");
		public void UpdateFastDeliveryMaxDistanceParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_maxDistanceToLatestTrackPointKm, value);

		#endregion
	}
}
