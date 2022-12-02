using System;

namespace Vodovoz.Parameters
{
	public class RouteListProfitabilitySettings : IRouteListProfitabilitySettings
	{
		private readonly IParametersProvider _parametersProvider;

		public RouteListProfitabilitySettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public decimal GetRouteListProfitabilityIndicatorInPercents =>
			_parametersProvider.GetValue<decimal>("route_list_profitability_indicator_in_percents");
	}
}
