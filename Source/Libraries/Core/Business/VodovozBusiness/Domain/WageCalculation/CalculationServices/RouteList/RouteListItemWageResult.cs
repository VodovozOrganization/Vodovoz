namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListItemWageResult
	{
		/// <summary>
		/// ЗП
		/// </summary>
		public decimal Wage { get; }
		/// <summary>
		/// Методика расчёта ЗП
		/// </summary>
		public WageDistrictLevelRate WageDistrictLevelRate { get; }

		public RouteListItemWageResult()
		{
			Wage = 0;
			WageDistrictLevelRate = null;
		}

		public RouteListItemWageResult(decimal wage)
		{
			Wage = wage;
			WageDistrictLevelRate = null;
		}

		public RouteListItemWageResult(decimal wage, WageDistrictLevelRate wageDistrictLevelRate)
		{
			Wage = wage;
			WageDistrictLevelRate = wageDistrictLevelRate;
		}
	}
}
