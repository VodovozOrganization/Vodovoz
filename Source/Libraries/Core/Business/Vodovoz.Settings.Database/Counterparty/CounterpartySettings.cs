using Vodovoz.Settings.Counterparty;

namespace Vodovoz.Settings.Database.Counterparty
{
	public class CounterpartySettings : ICounterpartySettings
	{
		private readonly ISettingsController _settingsController;

		public CounterpartySettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public int CounterpartyFromTenderId => _settingsController.GetValue<int>(nameof(CounterpartyFromTenderId).FromPascalCaseToSnakeCase());
		public int GetMobileAppCounterpartyCameFromId => _settingsController.GetIntValue("mobile_app_counterparty_came_from_id");
		public int GetWebSiteCounterpartyCameFromId => _settingsController.GetIntValue("web_site_counterparty_came_from_id");
		public string RevenueServiceClientAccessToken => _settingsController.GetStringValue("RevenueServiceClientAccessToken");
		public int ReferFriendPromotionCameFromId => _settingsController.GetValue<int>(nameof(ReferFriendPromotionCameFromId));
	}
}
