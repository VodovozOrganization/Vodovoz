using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace VodovozInfrastructureTests.Utils {
    [TestFixture()]
    public class NumbersUtilsTest {
        private static IEnumerable StringsWithNumbers()
        {
            yield return new object[] { "д 18 стр 1", new List<int>(new int[] {18,1}) };
            yield return new object[] { "0000", new List<int>(new int[] {0}) };
            yield return new object[] { "1 2 345 67 8 9 0", new List<int>(new int[] {1, 2, 345, 67, 8, 9, 0}) };
        }
        
        [Test(Description = "Парсинг чисел из строки")]
        [TestCaseSource(nameof(StringsWithNumbers))]
        public void ReturnOnlyNumbersFromString(string number, List<int> result)
        {
            // arrange
            string processedNumber;
            bool needBoth;

            // act
            var numbersFromString = VodovozInfrastructure.Utils.NumbersUtils.GetNumbersFromString(number).ToList();

            //assert
            Assert.That(numbersFromString.SequenceEqual(result));
        }
    }
}