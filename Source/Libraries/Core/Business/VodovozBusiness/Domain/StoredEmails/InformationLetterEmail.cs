using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	/// <summary>
	/// Информационное письмо
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Neuter,
		Accusative = "информационное письмо",
		AccusativePlural = "информационные письма",
		Genitive = "информационного письма",
		GenitivePlural = "информационных писем",
		Nominative = "информационное письмо",
		NominativePlural = "информационные письма",
		Prepositional = "информационном письме",
		PrepositionalPlural = "информационных письмах")]
	public class InformationLetterEmail : CounterpartyEmail
	{
        public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.InformationLetter;
    }
}
