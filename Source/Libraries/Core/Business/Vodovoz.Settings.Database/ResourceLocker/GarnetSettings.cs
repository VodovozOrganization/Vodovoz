using System;
using Vodovoz.Settings.ResourceLocker;

namespace Vodovoz.Settings.Database.ResourceLocker
{
	public class GarnetSettings : IGarnetSettings
	{
		private readonly ISettingsController _settingsController;

		public GarnetSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string Url => _settingsController.GetStringValue("GarnetUrl");

		public string Password => _settingsController.GetStringValue("GarnetPassword");

		public string ConnectionString => $"{Url},password={Password}";
	}
}
