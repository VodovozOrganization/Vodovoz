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
	}
}
