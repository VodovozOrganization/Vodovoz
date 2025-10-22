using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	public class OsrmSettings : IOsrmSettings
	{
		private readonly ISettingsController _settingsController;

		public OsrmSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		public string OsrmServiceUrl => _settingsController.GetStringValue("osrm_url_delivery_rule");
		public bool ExcludeToll => _settingsController.GetBoolValue("osrm_exclude_toll");
	}
}
