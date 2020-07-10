using System;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListWageCalculationService : IRouteListWageCalculationService
	{
		private readonly EmployeeWageParameter wageParameter;
		private readonly IRouteListWageCalculationSource source;
		private IRouteListWageCalculationService calculationService;
		
		public RouteListWageCalculationService(EmployeeWageParameter wageParameter, IRouteListWageCalculationSource source)
		{
			this.wageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
			this.source = source ?? throw new ArgumentNullException(nameof(source));

			if(source.DriverOfOurCar) {
				SetWageCalculationService(wageParameter.DriverWithOurCarsWageParameterItem);
			}
			else {
				SetWageCalculationService(wageParameter.WageParameterItem);
			}
		}

		private void SetWageCalculationService(WageParameterItem wageParameterItem)
		{
			switch(wageParameter.WageParameterItem.WageParameterItemType) {
				case WageParameterItemTypes.Manual:
					calculationService = new RouteListManualWageCalculationService((ManualWageParameterItem) wageParameterItem, source);
					break;
				case WageParameterItemTypes.OldRates:
					calculationService = new RouteListOldRatesWageCalculationService((OldRatesWageParameterItem) wageParameterItem, source);
					break;
				case WageParameterItemTypes.Fixed:
					calculationService = new RouteListFixedWageCalculationService((FixedWageParameterItem) wageParameterItem, source);
					break;
				case WageParameterItemTypes.Percent:
					calculationService = new RouteListPercentWageCalculationService((PercentWageParameterItem) wageParameterItem, source);
					break;
				case WageParameterItemTypes.RatesLevel:
					calculationService = new RouteListRatesLevelWageCalculationService((RatesLevelWageParameterItem) wageParameterItem, source);
					break;
				default:
					throw new ArgumentOutOfRangeException($"Пропущен один из типов {nameof(WageParameterItemTypes)}");
			}
		}
		
		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource source)
		{
			return calculationService.CalculateWageForRouteListItem(source);
		}

		public RouteListWageResult CalculateWage()
		{
			return calculationService.CalculateWage();
		}
	}
}