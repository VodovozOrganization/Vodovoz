using System;
using Vodovoz.Settings;

namespace Vodovoz.Parameters
{
	public class CounterpartySettings : ICounterpartySettings
	{
		private readonly ISettingsController _settingsController;

		public CounterpartySettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetMobileAppCounterpartyCameFromId => _settingsController.GetIntValue("mobile_app_counterparty_came_from_id");
		public int GetWebSiteCounterpartyCameFromId => _settingsController.GetIntValue("web_site_counterparty_came_from_id");
        public string RevenueServiceClientAccessToken => _parametersProvider.GetStringValue("RevenueServiceClientAccessToken");
	}
}
