using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Претензионное письмо
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Neuter,
		Accusative = "претензионное письмо",
		AccusativePlural = "претензионные письма",
		Genitive = "претензионного письма",
		GenitivePlural = "претензионных писем",
		Nominative = "претензионное письмо",
		NominativePlural = "претензионные письма",
		Prepositional = "претензионном письме",
		PrepositionalPlural = "претензионных письмах")]
	public class LetterOfClaimEmail : CounterpartyEmail
	{
		/// <inheritdoc />
		public override IEmailableDocument EmailableDocument { get; }

		/// <inheritdoc />
		public override CounterpartyEmailType Type => CounterpartyEmailType.LetterOfClaim;
	}
}
