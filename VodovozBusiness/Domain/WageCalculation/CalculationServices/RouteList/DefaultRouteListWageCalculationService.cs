namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class DefaultRouteListWageCalculationService : IRouteListWageCalculationService
	{
		public DefaultRouteListWageCalculationService() { }

		public RouteListWageResult CalculateWage() => new RouteListWageResult();

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource source) => new RouteListItemWageResult();
	}
}
