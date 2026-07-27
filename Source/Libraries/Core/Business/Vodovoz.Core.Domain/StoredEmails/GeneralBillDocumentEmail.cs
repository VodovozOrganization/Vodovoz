using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Общий счет по Email
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "общие счета",
		Nominative = "общий счет")]
	public class GeneralBillDocumentEmail : CounterpartyEmail
	{
        public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.GeneralBillDocument;
    }
}
