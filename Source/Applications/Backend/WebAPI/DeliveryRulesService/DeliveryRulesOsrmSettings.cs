using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;

namespace DeliveryRulesService
{
	public class DeliveryRulesOsrmSettings : IOsrmSettings
	{
		private readonly ISettingsController _settingsController;

		public DeliveryRulesOsrmSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		public string OsrmServiceUrl => _settingsController.GetStringValue("osrm_url_for_delivery_rules");
		public bool ExcludeToll => _settingsController.GetBoolValue("osrm_exclude_toll");
	}
}
