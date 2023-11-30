using System;
using Vodovoz.Settings.Pacs;

namespace Vodovoz.Settings.Database.Pacs
{
	public class PacsSettings : IPacsSettings
	{
		private readonly ISettingsController _settingsController;

		public PacsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public TimeSpan OperatorInactivityTimeout => TimeSpan.FromMinutes(_settingsController.GetIntValue("Pacs.OperatorInactivityTimeout.Minutes"));

		public TimeSpan OperatorKeepAliveInterval => TimeSpan.FromMinutes(_settingsController.GetIntValue("Pacs.OperatorKeepAliveInterval.Minutes"));

		public TimeSpan CallEventsSeqCacheTimeout => TimeSpan.FromMinutes(_settingsController.GetIntValue("Pacs.CallEventsSeqCacheTimeout.Minutes"));

		public TimeSpan CallEventsSeqCacheCleanInterval => TimeSpan.FromMinutes(_settingsController.GetIntValue("Pacs.CallEventsSeqCacheCleanInterval.Minutes"));

		public string AdministratorApiUrl => _settingsController.GetStringValue("Pacs.AdministratorApiUrl");

		public string AdministratorApiKey => _settingsController.GetStringValue("Pacs.AdministratorApiKey");

		public string OperatorApiUrl => _settingsController.GetStringValue("Pacs.OperatorApiUrl");

		public string OperatorApiKey => _settingsController.GetStringValue("Pacs.OperatorApiKey");
	}
}
