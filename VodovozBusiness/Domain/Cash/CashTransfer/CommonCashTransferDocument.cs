using System;
using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.CashTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения денежных средств на сумму",
		Nominative = "документ перемещения денежных средств на сумму",
		Prepositional = "документе перемещения денежных средств на сумму",
		PrepositionalPlural = "документах перемещения общих денежных средств на сумму"
	)]
	public class CommonCashTransferDocument : CashTransferDocumentBase
	{
		public CommonCashTransferDocument()
		{
		}
	}
}
