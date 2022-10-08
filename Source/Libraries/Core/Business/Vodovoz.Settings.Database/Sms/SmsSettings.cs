using QS.Project.DB;
using System;
using Vodovoz.Settings.Sms;

namespace Vodovoz.Settings.Database.Sms
{
	public sealed class SmsSettings : ISmsSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly IDataBaseInfo _dataBaseInfo;

		public SmsSettings(ISettingsController settingsController, IDataBaseInfo dataBaseInfo)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			_dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
		}

		public string InternalSmsServiceUrl => _settingsController.GetStringValue("internal_sms_service_url");

		public string InternalSmsServiceApiKey => _settingsController.GetStringValue("internal_sms_service_api_key");

		public bool SmsSendingAllowed
		{
			get
			{
				var allowedDatabaseName = _settingsController.GetStringValue("internal_sms_enabled_database");
				return _dataBaseInfo.Name == allowedDatabaseName;
			}
		}
	}
}
