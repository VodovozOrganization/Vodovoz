using System;

namespace Vodovoz.Parameters
{
	public class RoboatsSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public RoboatsSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int DefaultCounterpartyNameId => _parametersProvider.GetIntValue("roboats_default_counterparty_name_id");
		public int DefaultCounterpartyPatronymicId => _parametersProvider.GetIntValue("roboats_default_counterparty_patronymic_id");
		public string DeliverySchedulesAudiofilesFolder => _parametersProvider.GetStringValue("roboats_delivery_schedule_audiofiles_path");
		public string AddressesAudiofilesFolder => _parametersProvider.GetStringValue("roboats_street_audiofiles_path");
		public string WaterTypesAudiofilesFolder => _parametersProvider.GetStringValue("roboats_water_type_audiofiles_path");
		public string CounterpartyNameAudiofilesFolder => _parametersProvider.GetStringValue("roboats_counterparty_name_audiofiles_path");
		public string CounterpartyPatronymicAudiofilesFolder => _parametersProvider.GetStringValue("roboats_counterparty_patronymic_audiofiles_path");
		public int MaxBanknoteForReturn => _parametersProvider.GetIntValue("roboats_max_banknote_for_return");
		public int CallRegistryAutoRefreshInterval => _parametersProvider.GetIntValue("roboats_call_registry_autorefresh_interval");
		public int OrdersInMonths => _parametersProvider.GetIntValue("roboats_orders_in_months");
		public bool RoboatsEnabled
		{
			get
			{
				return _parametersProvider.GetBoolValue("roboats_enabled");
			}

			set
			{
				_parametersProvider.CreateOrUpdateParameter("roboats_enabled", value ? "True" : "False");
			}
		}

		/// <summary>
		/// Время в течении которого звонок является активным, 
		/// по истечении которого звонок будет считаться устаревшим 
		/// и будет закрыть при следующей проверке устаревших звонков
		/// </summary>
		public int CallTimeout => _parametersProvider.GetIntValue("roboats_call_timeout");
		
		/// <summary>
		/// Интервал проверки устаревших звонков (в минутах)
		/// </summary>
		public int StaleCallCheckInterval => _parametersProvider.GetIntValue("roboats_stale_calls_check_interval");
	}
}
