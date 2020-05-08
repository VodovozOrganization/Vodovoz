using System;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListManualWageCalculationService : IRouteListWageCalculationService
	{
		private readonly ManualWageParameter wageParameter;
		private readonly IRouteListWageCalculationSource wageCalculationSource;

		public RouteListManualWageCalculationService(ManualWageParameter wageParameter, IRouteListWageCalculationSource wageCalculationSource)
		{
			this.wageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
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
