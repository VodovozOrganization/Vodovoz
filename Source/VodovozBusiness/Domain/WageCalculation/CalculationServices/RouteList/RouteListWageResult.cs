namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListWageResult
	{
		public decimal Wage { get; }
		public decimal FixedWage { get; }
		public WageDistrictLevelRate WageDistrictLevelRate { get; }

		public RouteListWageResult()
		{
			Wage = 0;
			FixedWage = 0;
			WageDistrictLevelRate = null;
		}

		public RouteListWageResult(decimal wage, decimal fixedWage)
		{
			Wage = wage;
			FixedWage = fixedWage;
			WageDistrictLevelRate = null;
		}

		public RouteListWageResult(decimal wage, decimal fixedWage, WageDistrictLevelRate wageDistrictLevelRate)
		{
			Wage = wage;
			FixedWage = fixedWage;
			WageDistrictLevelRate = wageDistrictLevelRate;
		}
	}
}
