using System;
using System.Collections;
using NUnit.Framework;

namespace VodovozInfrastructureTests.PhoneUtils
{
	[TestFixture()]
	public class PhoneUtilsTest
	{

		private static IEnumerable NumbersWithEightTestSource()
		{
			yield return new object[] { "89991112233", "9991112233" };
			yield return new object[] { "8-999-111-22-33", "9991112233" };
			yield return new object[] { "8 (999)-111-22-33", "9991112233" };
			yield return new object[] { "8qwe9wqe9ewq9qwe1ewq1qwe1ewq2qwe2ewq3qwe3", "9991112233" };
			yield return new object[] { "8 800 111 22 33", "8001112233" };
			yield return new object[] { "8 888 88 88", "8888888" };
		}

		private static IEnumerable NumbersWithSevenTestSource()
		{
			yield return new object[] { "+79991112233", "9991112233" };
			yield return new object[] { "+7-999-111-22-33", "9991112233" };
			yield return new object[] { "+7 (999)-111-22-33", "9991112233" };
			yield return new object[] { "+7qwe9wqe9ewq9qwe1ewq1qwe1ewq2qwe2ewq3qwe3", "9991112233" };
			yield return new object[] { "+7 800 111 22 33", "8001112233"};
			yield return new object[] { "788 88 88", "888888" };
		}

		private static IEnumerable NothingHappensTestSource()
		{
			yield return new object[] { "3434455", "3434455" };
			yield return new object[] { "0000", "0000" };
			yield return new object[] { "123456789", "123456789" };
		}

		[Test(Description = "Номера начинающиеся с восьмерки, второй бул параметр должен быть true, возвращаемое значение должно быть без начальной восьмерки")]
		[TestCaseSource(nameof(NumbersWithEightTestSource))]
		public void ReturnOnlyNumbersFromPhone_WithoutFirstEight(string number, string result)
		{
			// arrange
			string processedNumber;
			bool needBoth;

			// act
			processedNumber = VodovozInfrastructure.Utils.PhoneUtils.NumberTrim(number, out needBoth);

			//assert
			Assert.That(processedNumber == result && needBoth == true);
		}

		[Test(Description = "Номера начинающиеся с семерки, второй бул параметр должен быть false, возвращаемое значение должно быть цифрами без начальной семерки")]
		[TestCaseSource(nameof(NumbersWithSevenTestSource))]
		public void ReturnOnlyNumbersFromPhone_WithoutFirstSeven(string number, string result)
		{
			// arrange
			string processedNumber;
			bool needBoth;

			// act
			processedNumber = VodovozInfrastructure.Utils.PhoneUtils.NumberTrim(number, out needBoth);

			//assert
			Assert.That(processedNumber == result && needBoth == false);
		}

		[Test(Description = "Функция не должна обрабатывать данные строки")]
		[TestCaseSource(nameof(NothingHappensTestSource))]
		public void NothingHappens(string number, string result)
		{
			// arrange
			string processedNumber;
			bool needBoth;

			// act
			processedNumber = VodovozInfrastructure.Utils.PhoneUtils.NumberTrim(number, out needBoth);

			//assert
			Assert.That(processedNumber == result && needBoth == false);
		}
	}
}
