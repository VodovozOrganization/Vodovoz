using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Fias;

namespace Vodovoz.Settings.Database.Fias
{
	public class FiasApiSettings : IFiasApiSettings
	{
		private readonly ISettingsController _settingsController;

		public FiasApiSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string FiasApiBaseUrl => _settingsController.GetStringValue("FiasApiBaseUrl");
		public string FiasApiToken => _settingsController.GetStringValue("FiasApiToken");
	}
}
