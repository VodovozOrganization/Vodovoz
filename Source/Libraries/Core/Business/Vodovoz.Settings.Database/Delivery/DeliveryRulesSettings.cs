using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Settings.Database.Delivery
{
	public class DeliveryRulesSettings : IDeliveryRulesSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly IUnitOfWorkFactory _uowFactory;
		private const string _onlineDeliveriesTodayParameter = "is_stopped_online_deliveries_today";
		private const string _bottlesCountForFlyerParameter = "bottles_count_for_flyer";
		private const string _additionalLoadingFlyerAdditionEnabledParameter = "additional_loading_flyer_addition_enabled";
		private const string _carsMonitoringResfreshInSecondsParameter = "cars_monitoring_resfresh_in_seconds";

		private const string _additionalLoadingFlyerForNewCounterpartyBottlesCountParameter =
			"additional_loading_flyer_for_new_counterparty_bottles_count";
		private const string _additionalLoadingFlyerForNewCounterpartyEnabledParameter =
			"additional_loading_flyer_for_new_counterparty_enabled";
		private const string _specificTimeForMaxFastOrdersCountParameter = "fast_delivery_time_for_max_orders";
		private const string _driverUnloadTimeParameter = "fast_delivery_driver_unload_time";
		private const string _driverGoodWeightLiftPerHandInKgParameter = "fast_delivery_driver_weight_lift_kg";
		private const string _maxFastOrdersPerSpecificTimeParameter = "fast_delivery_max_orders_per_time";
		private const string _fastDeliveryScheduleIdParameter = "fast_delivery_schedule_id";
		private const string _maxTimeOffsetForLatestTrackPointParameter = "fast_delivery_time_offset";
		private const string _maxTimeForFastDeliveryParameter = "fast_delivery_time";
		private const string _minTimeForNewFastDeliveryOrderParameter = "fast_delivery_min_new_order_time";

		private const string _getMaxDistanceToLatestTrackPointKmUnitOfWorkTitle = "GetMaxDistanceToLatestTrackPointKm";
		private const string _setMaxDistanceToLatestTrackPointKmUnitOfWorkTitle = "SetMaxDistanceToLatestTrackPointKm";

		public DeliveryRulesSettings(ISettingsController settingsController, IUnitOfWorkFactory uowFactory)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public bool IsStoppedOnlineDeliveriesToday =>
			_settingsController.GetBoolValue(_onlineDeliveriesTodayParameter);

		public int BottlesCountForFlyer =>
			_settingsController.GetValue<int>(_bottlesCountForFlyerParameter);

		public bool AdditionalLoadingFlyerAdditionEnabled =>
			_settingsController.GetValue<bool>(_additionalLoadingFlyerAdditionEnabledParameter);

		public int FlyerForNewCounterpartyBottlesCount =>
			_settingsController.GetValue<int>(_additionalLoadingFlyerForNewCounterpartyBottlesCountParameter);

		public bool FlyerForNewCounterpartyEnabled =>
			_settingsController.GetValue<bool>(_additionalLoadingFlyerForNewCounterpartyEnabledParameter);

		public void UpdateOnlineDeliveriesTodayParameter(string value) =>
			_settingsController.CreateOrUpdateSetting(_onlineDeliveriesTodayParameter, value);

		public void UpdateBottlesCountForFlyerParameter(string value) =>
			_settingsController.CreateOrUpdateSetting(_bottlesCountForFlyerParameter, value);

		public void UpdateAdditionalLoadingFlyerAdditionEnabledParameter(string value) =>
			_settingsController.CreateOrUpdateSetting(_additionalLoadingFlyerAdditionEnabledParameter, value);

		public void UpdateFlyerForNewCounterpartyBottlesCountParameter(string value) =>
			_settingsController.CreateOrUpdateSetting(_additionalLoadingFlyerForNewCounterpartyBottlesCountParameter, value);

		public void UpdateFlyerForNewCounterpartyEnabledParameter(string value) =>
			_settingsController.CreateOrUpdateSetting(_additionalLoadingFlyerForNewCounterpartyEnabledParameter, value);

		public void UpdateMaxFastOrdersPerSpecificTimeParameter(string value) =>
			_settingsController.CreateOrUpdateSetting(_maxFastOrdersPerSpecificTimeParameter, value);

		#region FastDelivery

		public int DriverGoodWeightLiftPerHandInKg => _settingsController.GetValue<int>(_driverGoodWeightLiftPerHandInKgParameter);
		public int MaxFastOrdersPerSpecificTime => _settingsController.GetValue<int>(_maxFastOrdersPerSpecificTimeParameter);
		public int FastDeliveryScheduleId => _settingsController.GetValue<int>(_fastDeliveryScheduleIdParameter);
		public TimeSpan MaxTimeOffsetForLatestTrackPoint => _settingsController.GetValue<TimeSpan>(_maxTimeOffsetForLatestTrackPointParameter);
		public TimeSpan MaxTimeForFastDelivery => _settingsController.GetValue<TimeSpan>(_maxTimeForFastDeliveryParameter);
		public TimeSpan MinTimeForNewFastDeliveryOrder => _settingsController.GetValue<TimeSpan>(_minTimeForNewFastDeliveryOrderParameter);
		public TimeSpan DriverUnloadTime => _settingsController.GetValue<TimeSpan>(_driverUnloadTimeParameter);
		public TimeSpan SpecificTimeForMaxFastOrdersCount => _settingsController.GetValue<TimeSpan>(_specificTimeForMaxFastOrdersCountParameter);
		public double CarsMonitoringResfreshInSeconds => _settingsController.GetValue<double>(_carsMonitoringResfreshInSecondsParameter);

		#endregion
	}
}
