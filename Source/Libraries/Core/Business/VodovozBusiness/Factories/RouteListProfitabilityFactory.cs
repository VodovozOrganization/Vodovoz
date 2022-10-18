using Vodovoz.Domain.Profitability;

namespace Vodovoz.Factories
{
	public class RouteListProfitabilityFactory : IRouteListProfitabilityFactory
	{
		public RouteListProfitability CreateRouteListProfitability() => new RouteListProfitability();
	}
}
