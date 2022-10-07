using NSubstitute;
using NUnit.Framework;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList
{
	[TestFixture]
	public class RouteListFixedWageCalculationServiceTests
	{
		[Test(Description = "Расчёт ЗП для МЛ с фиксированными ЗП")]
		public void WageCalculationForRouteListWithFixedWage()
		{
			// arrange
			FixedWageParameterItem fixedWage = Substitute.For<FixedWageParameterItem>();
			fixedWage.RouteListFixedWage.Returns(1112);
			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();

			IRouteListWageCalculationService wageCalculationService = new RouteListFixedWageCalculationService(
				fixedWage,
				src
			);

			// act
			var result = wageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(1112));
			Assert.That(result.FixedWage, Is.EqualTo(1112));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}
	}
}