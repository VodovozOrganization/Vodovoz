using System.Linq;
using NUnit.Framework;
using Vodovoz.Domain.Client;
namespace VodovozBusinessTests.Domain.Client
{
	[TestFixture]
	public class CounterpartyActivityKindTests
	{
		[Test(Description = "Метод преобразует многострочный текст из свойства в лист строк")]
		public void GetListOfSubstrings_NotBeautyStringInSubstringProperty_TransformsToListOfStrings()
		{
			// arrange
			var counterpartyActivityKindSet = new CounterpartyActivityKind {
				Substrings = "  AAA \nbBb    \n    ccccC \n   \n\n  DD dddD\n\n\n  1\n  \n"
			};

			// act
			var testResult = counterpartyActivityKindSet.GetListOfSubstrings().Select(x => x.Substring);

			// assert
			Assert.That(new[] { "aaa", "bbb", "ccccc", "dd dddd", "1" }, Is.EquivalentTo(testResult));
		}
	}
}
