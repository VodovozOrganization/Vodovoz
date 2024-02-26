using System;
using Vodovoz.Settings.Roboats;

namespace Vodovoz.Settings.Database.Roboats
{
	public class RoboatsSettings : IRoboatsSettings
	{
		private readonly ISettingsController _settingsController;

		public RoboatsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DefaultCounterpartyNameId => _settingsController.GetIntValue("roboats_default_counterparty_name_id");
		public int DefaultCounterpartyPatronymicId => _settingsController.GetIntValue("roboats_default_counterparty_patronymic_id");
		public string DeliverySchedulesAudiofilesFolder => _settingsController.GetStringValue("roboats_delivery_schedule_audiofiles_path");
		public string AddressesAudiofilesFolder => _settingsController.GetStringValue("roboats_street_audiofiles_path");
		public string WaterTypesAudiofilesFolder => _settingsController.GetStringValue("roboats_water_type_audiofiles_path");
		public string CounterpartyNameAudiofilesFolder => _settingsController.GetStringValue("roboats_counterparty_name_audiofiles_path");
		public string CounterpartyPatronymicAudiofilesFolder => _settingsController.GetStringValue("roboats_counterparty_patronymic_audiofiles_path");
		public int MaxBanknoteForReturn => _settingsController.GetIntValue("roboats_max_banknote_for_return");
		public int CallRegistryAutoRefreshInterval => _settingsController.GetIntValue("roboats_call_registry_autorefresh_interval");
		public int OrdersInMonths => _settingsController.GetIntValue("roboats_orders_in_months");
		public bool RoboatsEnabled
		{
			get => _settingsController.GetBoolValue("roboats_enabled");
			set => _settingsController.CreateOrUpdateSetting("roboats_enabled", value ? "True" : "False");
		}

		/// <summary>
		/// Время в течении которого звонок является активным, 
		/// по истечении которого звонок будет считаться устаревшим 
		/// и будет закрыть при следующей проверке устаревших звонков
		/// </summary>
		public int CallTimeout => _settingsController.GetIntValue("roboats_call_timeout");

		/// <summary>
		/// Интервал проверки устаревших звонков (в минутах)
		/// </summary>
		public int StaleCallCheckInterval => _settingsController.GetIntValue("roboats_stale_calls_check_interval");
	}
}
