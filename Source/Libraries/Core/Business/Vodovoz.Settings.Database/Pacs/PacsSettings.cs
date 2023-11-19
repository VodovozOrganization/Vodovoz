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

		public TimeSpan OperatorInactivityTimeout => TimeSpan.FromSeconds(_settingsController.GetIntValue("Pacs.OperatorInactivityTimeout"));

		public TimeSpan OperatorKeepAliveInterval => TimeSpan.FromSeconds(_settingsController.GetIntValue("Pacs.OperatorKeepAliveInterval"));

		public string AdministratorApiUrl => _settingsController.GetStringValue("Pacs.AdministratorApiUrl");

		public string AdministratorApiKey => _settingsController.GetStringValue("Pacs.AdministratorApiKey");
	}
}
