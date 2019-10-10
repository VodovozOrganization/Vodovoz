using System;
namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListFixedWageCalculationService : IRouteListWageCalculationService
	{
		private readonly FixedWageParameter wageParameter;
		private readonly IRouteListWageCalculationSource wageCalculationSource;

		public RouteListFixedWageCalculationService(FixedWageParameter wageParameter, IRouteListWageCalculationSource wageCalculationSource)
		{
			this.wageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
			this.wageCalculationSource = wageCalculationSource ?? throw new ArgumentNullException(nameof(wageCalculationSource));
		}

		public RouteListWageResult CalculateWage()
		{
			switch(wageParameter.FixedWageType) {
				case FixedWageTypes.RouteList:
					return new RouteListWageResult(wageParameter.RouteListFixedWage, wageParameter.RouteListFixedWage);
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
