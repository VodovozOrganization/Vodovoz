using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Settings.Database.Logistics
{
	public class RouteListProfitabilitySettings : IRouteListProfitabilitySettings
	{
		private readonly ISettingsController _settingsController;

		public RouteListProfitabilitySettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public decimal GetRouteListProfitabilityIndicatorInPercents =>
			_settingsController.GetValue<decimal>("route_list_profitability_indicator_in_percents");
	}
}
