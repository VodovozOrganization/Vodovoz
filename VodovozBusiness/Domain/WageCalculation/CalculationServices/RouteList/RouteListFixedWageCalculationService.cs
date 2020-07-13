using System;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListFixedWageCalculationService : IRouteListWageCalculationService
	{
		private readonly FixedWageParameterItem wageParameterItem;
		private readonly IRouteListWageCalculationSource wageCalculationSource;

		public RouteListFixedWageCalculationService(FixedWageParameterItem wageParameterItem, IRouteListWageCalculationSource wageCalculationSource)
		{
			this.wageParameterItem = wageParameterItem ?? throw new ArgumentNullException(nameof(wageParameterItem));
			this.wageCalculationSource = wageCalculationSource ?? throw new ArgumentNullException(nameof(wageCalculationSource));
		}

		public RouteListWageResult CalculateWage()
		{
			switch(wageParameterItem.FixedWageType) {
				case FixedWageTypes.RouteList:
					return new RouteListWageResult(wageParameterItem.RouteListFixedWage, wageParameterItem.RouteListFixedWage);
				default:
					throw new NotImplementedException();
			}
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource source)
		{
			return new RouteListItemWageResult();
		}
	}
}
