using System.Collections.Generic;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class DefaultRouteListWageCalculationService : IRouteListWageCalculationService
	{
		public DefaultRouteListWageCalculationService() { }

		public RouteListWageResult CalculateWage() => new RouteListWageResult();

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource source) => new RouteListItemWageResult();

		public RouteListItemWageCalculationDetails GetWageCalculationDetailsForRouteListItem(IRouteListItemWageCalculationSource source)
		{
			RouteListItemWageCalculationDetails addressWageDetails = new RouteListItemWageCalculationDetails()
			{
				RouteListItemWageCalculationName = "Способ расчёта ЗП по умолчанию",
				WageCalculationEmployeeCategory = source.EmployeeCategory
			};

			return addressWageDetails;
		}
	}
}
