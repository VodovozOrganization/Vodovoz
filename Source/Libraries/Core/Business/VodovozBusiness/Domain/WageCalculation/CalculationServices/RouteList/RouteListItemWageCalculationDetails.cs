using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListItemWageCalculationDetails
	{
		public string RouteListItemWageCalculationName { get; set; }
		public EmployeeCategory WageCalculationEmployeeCategory { get; set; }
		public IList<WageCalculationDetailsItem> WageCalculationDetailsList { get; set; } = new List<WageCalculationDetailsItem>();
	}

	public class WageCalculationDetailsItem
	{
		public string Name { get; set; }
		public decimal Price { get; set; }
		public int Count { get; set; }
	}
}
