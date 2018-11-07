using NUnit.Framework;
using System;
using Vodovoz.Domain.Client;

namespace VodovozBusinessTests.Counterparties
{
	[TestFixture(TestOf = typeof(Counterparty))]
	public class CounterpartyTests
	{
		[Test()]
		public void IsNotEmptyTestCase()
		{
			Counterparty subjectEmpty = new Counterparty();
			Counterparty subjectFilled = new Counterparty();
			subjectFilled.CargoReceiver = "CargoReceiver";
			subjectFilled.SpecialCustomer = "SpecialCustomer";
			subjectFilled.SpecialContractNumber = "SpecialContractNumber";
			subjectFilled.SpecialKPP = "SpecialKPP";
			subjectFilled.GovContract = "GovContract";
			subjectFilled.SpecialDeliveryAddress = "SpecialDeliveryAddress";

			Assert.IsTrue(subjectFilled.IsNotEmpty && !subjectEmpty.IsNotEmpty);
		}
	}
}
