namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public interface IRouteListWageCalculationService : IWageCalculationService<RouteListWageResult>
	{
		RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource source);

		RouteListItemWageCalculationDetails GetWageCalculationDetailsForRouteListItem(IRouteListItemWageCalculationSource source);
	}
}
