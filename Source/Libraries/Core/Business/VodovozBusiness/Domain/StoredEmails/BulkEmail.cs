using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "массовая рассылка",
		Nominative = "массовая рассылка")]
	public class BulkEmail : CounterpartyEmail
	{
		public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.Bulk;
	}
}
