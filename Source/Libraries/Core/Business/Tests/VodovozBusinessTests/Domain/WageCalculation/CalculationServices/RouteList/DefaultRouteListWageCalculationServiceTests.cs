using NUnit.Framework;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList
{
	[TestFixture]
	public class DefaultRouteListWageCalculationServiceTests
	{
		[Test(Description = "Расчёт ЗП для ручного расчёта. Всегда 0.")]
		public void WageCalculationForDefault_ReturnsZero()
		{
			// arrange
			IRouteListWageCalculationService manualWageCalculationService = new DefaultRouteListWageCalculationService();

			// act
			var result = manualWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(0));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}
	}
}