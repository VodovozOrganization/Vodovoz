using System;
using System.Collections;
using NUnit.Framework;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList.AdvancedWageParameters
{
	[TestFixture]
	public class BottlesCountAdvancedWageParameterTest
	{
		static IEnumerable ConflictBetwenParamsGroup()
		{
			var param1 = new BottlesCountAdvancedWageParameter 
				{ 
					BottlesFrom = 1, 
					LeftSing = ComparisonSings.Equally 
				}; // 1 = x
			var param2 = new BottlesCountAdvancedWageParameter 
				{ 
					BottlesFrom = 1, 
					LeftSing = ComparisonSings.MoreOrEqual 
				}; // 1 >= x
			var param3 = new BottlesCountAdvancedWageParameter 
				{ 
					BottlesFrom = 5, 
					LeftSing = ComparisonSings.LessOrEqual, 
					RightSing = ComparisonSings.Less, 
					BottlesTo = 6 
				}; // 5 <= x < 6
			var param4 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 5,
				LeftSing = ComparisonSings.More,
				RightSing = ComparisonSings.Less,
				BottlesTo = 6
			}; // 5 > x < 6

			yield return new TestCaseData(param1, param2).Returns(true).SetDescription($"{param1} VS {param2}");
			yield return new TestCaseData(param1, param3).Returns(false).SetDescription($"{param1} VS {param3}");
			yield return new TestCaseData(param1, param4).Returns(true).SetDescription($"{param1} VS {param4}");
			yield return new TestCaseData(param2, param3).Returns(false).SetDescription($"{param2} VS {param3}");
			yield return new TestCaseData(param2, param4).Returns(true).SetDescription($"{param2} VS {param4}");
			yield return new TestCaseData(param3, param4).Returns(false).SetDescription($"{param3} VS {param4}");
		}
		[TestCaseSource(nameof(ConflictBetwenParamsGroup))]
		[Test(Description = "Проверка на конфликт между двумя параметрами")]
		public bool ConflictBetweenParam(BottlesCountAdvancedWageParameter param1, BottlesCountAdvancedWageParameter param2)
		{
			return param1.HasConflicWith(param2);
		}

		static IEnumerable CheckParameterRangeCalculationParams()
		{
			var param1 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 1,
				LeftSing = ComparisonSings.Equally
			}; // 1 = x
			var param2 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 1,
				LeftSing = ComparisonSings.MoreOrEqual
			}; // 1 >= x
			var param3 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 5,
				LeftSing = ComparisonSings.LessOrEqual,
				RightSing = ComparisonSings.Less,
				BottlesTo = 6
			}; // 5 <= x < 6
			var param4 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 5,
				LeftSing = ComparisonSings.More,
				RightSing = ComparisonSings.Less,
				BottlesTo = 6
			}; // 5 > x <= 6
			var param5 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 5,
				LeftSing = ComparisonSings.Equally,
				RightSing = ComparisonSings.Less,
				BottlesTo = 6
			}; // 5 = x < 6
			var param6 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 2,
				LeftSing = ComparisonSings.Equally,
				RightSing = ComparisonSings.Less,
			}; // 2 = x < null 
			var param7 = new BottlesCountAdvancedWageParameter {
				BottlesFrom = 4,
				LeftSing = ComparisonSings.More,
				BottlesTo = 6
			}; // 4 > x null 6

			yield return new TestCaseData(param1).Returns((1,1)).SetDescription($"{param1}");
			yield return new TestCaseData(param2).Returns((0,1)).SetDescription($"{param2}");
			yield return new TestCaseData(param3).Returns((5,5)).SetDescription($"{param3}");
			yield return new TestCaseData(param4).Returns((0,4)).SetDescription($"{param4}");
			yield return new TestCaseData(param5).Returns((5, 5)).SetDescription($"{param5}");
			yield return new TestCaseData(param6).Returns((2, 2)).SetDescription($"{param6}");
			yield return new TestCaseData(param7).Returns((0,3)).SetDescription($"{param7}");
		}
		[TestCaseSource(nameof(CheckParameterRangeCalculationParams))]
		[Test(Description = "Проверка расчета диапазона бутылей в параметре")]
		public (uint,uint) CheckParameterRangeCalculation(BottlesCountAdvancedWageParameter param)
		{
			return param.GetCountRange();
		}


		[Test(Description = "Проверка расчета диапазона бутылей в параметре(при некорректных параметрах)")]
		public void CheckParameterRangeCalculationWithInvalidParams()
		{
			// arrange
			var param1 = new BottlesCountAdvancedWageParameter {
				BottlesTo = 10,
				LeftSing = ComparisonSings.Less,
				RightSing = ComparisonSings.Less,
				BottlesFrom = 10
			};
			var param2 = new BottlesCountAdvancedWageParameter {
				BottlesTo = 1,
				LeftSing = ComparisonSings.Less,
				RightSing = ComparisonSings.MoreOrEqual,
				BottlesFrom = 5
			};
			var param3 = new BottlesCountAdvancedWageParameter {
				BottlesTo = 1,
				LeftSing = ComparisonSings.Less,
				RightSing = ComparisonSings.More,
				BottlesFrom = 5
			};

			// assert //act
			Assert.Throws(typeof(ArgumentException), () => param1.GetCountRange());
			Assert.Throws(typeof(ArgumentException), () => param2.GetCountRange());
			Assert.Throws(typeof(ArgumentException), () => param3.GetCountRange());
		}

		[Test(Description = "Проверка конфликта между параметров разных типов")]
		public void CheckingСonflictBetweenParametersOfDifferentTypes()
		{
			// arrange
			AdvancedWageParameter bottleParam = new BottlesCountAdvancedWageParameter();
			AdvancedWageParameter deliveryTimeParam = new DeliveryTimeAdvancedWageParameter();

			// act
			bool result = bottleParam.HasConflicWith(deliveryTimeParam);

			// assert
			Assert.IsTrue(result);
		}

	}
}
