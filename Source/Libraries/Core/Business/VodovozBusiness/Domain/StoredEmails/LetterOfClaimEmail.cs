using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;

namespace VodovozBusiness.Domain.StoredEmails
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
