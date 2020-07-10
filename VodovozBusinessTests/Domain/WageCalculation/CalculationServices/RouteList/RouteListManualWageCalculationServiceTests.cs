using NSubstitute;
using NUnit.Framework;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList
{
	[TestFixture]
	public class RouteListManualWageCalculationServiceTests
	{
		[Test(Description = "Расчёт ЗП для ручного расчёта. Всегда 0.")]
		public void WageCalculationForNoWage_ReturnsZero()
		{
			// arrange
			ManualWageParameterItem manualWage = Substitute.For<ManualWageParameterItem>();
			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();

			IRouteListWageCalculationService manualWageCalculationService = new RouteListManualWageCalculationService(
				manualWage,
				src
			);

			// act
			var result = manualWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(0));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}
	}
}
