using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Settings.Database.Delivery
{
	public class FastDeliveryAvailabilityHistorySettings : IFastDeliveryAvailabilityHistorySettings
	{
		private const string _fastDeliveryHistoryClearDate = "fast_delivery_availability_history_clear_date";
		private readonly ISettingsController _settingsController;

		public FastDeliveryAvailabilityHistorySettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		public void UpdateFastDeliveryHistoryClearDate(string value)
		{
			_settingsController.CreateOrUpdateSetting(_fastDeliveryHistoryClearDate, value);
		}

		public int FastDeliveryHistoryStorageDays => _settingsController.GetValue<int>("fast_delivery_availability_history_storage_days");
		public DateTime FastDeliveryHistoryClearDate => _settingsController.GetValue<DateTime>(_fastDeliveryHistoryClearDate);
	}
}
