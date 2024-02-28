using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	public class GlobalSettings : IGlobalSettings
	{
		private readonly ISettingsController _settingsController;

		public GlobalSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		public string OsrmServiceUrl => _settingsController.GetStringValue("osrm_url");
		public bool ExcludeToll => _settingsController.GetBoolValue("osrm_exclude_toll");
	}
}
