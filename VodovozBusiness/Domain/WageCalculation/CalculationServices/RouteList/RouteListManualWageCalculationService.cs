using System;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListManualWageCalculationService : IRouteListWageCalculationService
	{
		private readonly ManualWageParameterItem wageParameterItem;
		private readonly IRouteListWageCalculationSource wageCalculationSource;

		public RouteListManualWageCalculationService(ManualWageParameterItem wageParameterItem, IRouteListWageCalculationSource wageCalculationSource)
		{
			this.wageParameterItem = wageParameterItem ?? throw new ArgumentNullException(nameof(wageParameterItem));
			this.wageCalculationSource = wageCalculationSource ?? throw new ArgumentNullException(nameof(wageCalculationSource));
		}

		public RouteListWageResult CalculateWage()
		{
			return new RouteListWageResult();
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource source)
		{
			return new RouteListItemWageResult();
		}
	}
}
