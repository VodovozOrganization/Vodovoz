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
		private int _organizationId;
		/// <inheritdoc />
		public override IEmailableDocument EmailableDocument { get; }

		/// <inheritdoc />
		public override CounterpartyEmailType Type => CounterpartyEmailType.LetterOfClaim;

		/// <summary>
		/// Организация, от которой будет отправлено письмо
		/// </summary>
		[Display(Name = "Организация")]
		public virtual int OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}
	}
}
