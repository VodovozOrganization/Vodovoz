using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Settings.Database.Delivery
{
	public class FastDeliveryAvailabilityHistorySettings : IFastDeliveryAvailabilityHistorySettings
	{
		private readonly ISettingsController _settingsController;

		public FastDeliveryAvailabilityHistorySettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int FastDeliveryHistoryStorageDays => _settingsController.GetValue<int>("fast_delivery_availability_history_storage_days");
	}
}
