using NUnit.Framework;
using System;
using System.Collections;

namespace Vodovoz.Domain.Common.Tests
{
	[TestFixture()]
	public class CoordinateTests
	{
		public new static IEnumerable ValidCoordinateValues
		{
			get
			{
				yield return new object[] { "0, 0", new Coordinate(0M, 0M)};
				yield return new object[] { "-0, -0", new Coordinate(0M, 0M) };
				yield return new object[] { "90, 180", new Coordinate(90M, 180M) };
				yield return new object[] { "-90, -180", new Coordinate(-90M, -180M) };
				yield return new object[] { "50.000000, -60.000000", new Coordinate(50M, -60M) };
				yield return new object[] { "50.123456, -60.123456", new Coordinate(50.123456M, -60.123456M) };
				yield return new object[] { "50.000000; -60.000000", new Coordinate(50M, -60M) };
				yield return new object[] { "50.000000: -60.000000", new Coordinate(50M, -60M) };
				yield return new object[] { "50,000000, -60,000000", new Coordinate(50M, -60M) };
				yield return new object[] { "50,0000001, -60,000000", new Coordinate(50M, -60M) };
				yield return new object[] { "50,000000, -60,0000001", new Coordinate(50M, -60M) };
			}
		}

		public new static IEnumerable InvalidCoordinateValues
		{
			get
			{
				yield return new object[] { "" };
				yield return new object[] { " " };
				yield return new object[] { null };
				yield return new object[] { "1" };
				yield return new object[] { "aaaa" };
				yield return new object[] { "a, a" };
				yield return new object[] { "90.000001, 180" };
				yield return new object[] { "-90.000001, 180" };
				yield return new object[] { "0, 180.000001" };
				yield return new object[] { "0, -180.000001" };
				yield return new object[] { "50.000.000, -60.000.000" };
			}
		}

		[TestCaseSource(nameof(ValidCoordinateValues))]
		[Test(Description = "Парсинг правильных значений координат")]
		public void ParseValidValuesTest(string coordinateStringValue, object resultValue)
		{
			//arrange
			//act
			var coordinate = Coordinate.Parse(coordinateStringValue);

			//assert
			Assert.That(coordinate, Is.EqualTo(resultValue));
		}

		

		[TestCaseSource(nameof(InvalidCoordinateValues))]
		[Test(Description = "Парсинг неправильных значений координат")]
		public void ParseInvalidValuesTest(string coordinateStringValue)
		{
			//arrange
			//act
			TestDelegate act = new TestDelegate(() => {
				var coordinate = Coordinate.Parse(coordinateStringValue);
			});

			//assert
			Assert.Throws(typeof(InvalidOperationException), act);
		}

		[TestCaseSource(nameof(ValidCoordinateValues))]
		[Test(Description = "Попытка распарсить правильные значения координат")]
		public void TryParseValidValuesTest(string coordinateStringValue, object resultValue)
		{
			//arrange
			//act
			var result = Coordinate.TryParse(coordinateStringValue, out var coordinate);

			//assert
			if(!result)
			{
				Assert.Fail($"TryParse return false, input value: \"{coordinateStringValue}\", expected: \"{resultValue}\"");
			}
			Assert.That(coordinate, Is.EqualTo(resultValue));
		}

		[TestCaseSource(nameof(InvalidCoordinateValues))]
		[Test(Description = "Попытка распарсить неправильные значения координат")]
		public void TryParseInvalidValuesTest(string coordinateStringValue)
		{
			//arrange
			//act
			var result = Coordinate.TryParse(coordinateStringValue, out var coordinate);

			//assert
			Assert.That(result, Is.False);
		}
	}
}