using System;
using Vodovoz.Settings.Sms;

namespace Vodovoz.Settings.Database.Sms
{
	public sealed class SmsSettings : ISmsSettings
	{
		private readonly ISettingsController _settingsController;

		public SmsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string InternalSmsServiceUrl => _settingsController.GetStringValue("internal_sms_service_url");

		public string InternalSmsServiceApiKey => _settingsController.GetStringValue("internal_sms_service_api_key");
	}
}
