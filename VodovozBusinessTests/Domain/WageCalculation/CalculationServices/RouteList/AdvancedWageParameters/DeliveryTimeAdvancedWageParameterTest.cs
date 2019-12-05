using System;
using System.Collections;
using NUnit.Framework;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList.AdvancedWageParameters
{
	[TestFixture]
	public class DeliveryTimeAdvancedWageParameterTest
	{
		static IEnumerable ConflictBetwenParamsGroup()
		{
			var param1 = new DeliveryTimeAdvancedWageParameter {
				StartTime = new TimeSpan(0, 0, 0),
				EndTime = new TimeSpan(0,0,59)
			};
			var param2 = new DeliveryTimeAdvancedWageParameter {
				StartTime = new TimeSpan(1, 0, 0),
				EndTime = new TimeSpan(4, 30, 0)
			};
			var param3 = new DeliveryTimeAdvancedWageParameter {
				StartTime = new TimeSpan(0, 0, 0),
				EndTime = new TimeSpan(23, 59, 59)
			};
			var param4 = new DeliveryTimeAdvancedWageParameter {
				StartTime = new TimeSpan(2, 1, 1),
				EndTime = new TimeSpan(3, 10, 2)
			};

			yield return new TestCaseData(param1, param2).Returns(false).SetName($"{param1} VS {param2}");
			yield return new TestCaseData(param1, param3).Returns(true).SetName($"{param1} VS {param3}");
			yield return new TestCaseData(param1, param4).Returns(false).SetName($"{param1} VS {param4}");
			yield return new TestCaseData(param2, param3).Returns(true).SetName($"{param2} VS {param3}");
			yield return new TestCaseData(param2, param4).Returns(true).SetName($"{param2} VS {param4}");
			yield return new TestCaseData(param3, param4).Returns(true).SetName($"{param3} VS {param4}");
		}
		[TestCaseSource(nameof(ConflictBetwenParamsGroup))]
		[Test(Description = "Проверка на конфликт между двумя параметрами")]
		public bool ConflictBetweenParam(DeliveryTimeAdvancedWageParameter param1, DeliveryTimeAdvancedWageParameter param2)
		{
			return param1.HasConflicWith(param2);
		}

		[Test(Description = "Проверка конфликта между параметров разных типов")]
		public void CheckingСonflictBetweenParametersOfDifferentTypes()
		{
			// arrange
			AdvancedWageParameter bottleParam = new BottlesCountAdvancedWageParameter();
			AdvancedWageParameter deliveryTimeParam = new DeliveryTimeAdvancedWageParameter();

			// act
			bool result = deliveryTimeParam.HasConflicWith(bottleParam);

			// assert
			Assert.IsTrue(result);
		}

	}
}
