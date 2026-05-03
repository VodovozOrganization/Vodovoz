using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;

namespace VodovozBusiness.Domain.StoredEmails
{
	/// <summary>
	/// Массовая рассылка от указанной организации
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "массовая рассылка от указанной организации",
		AccusativePlural = "массовые рассылки от указанных организаций",
		Genitive = "массовой рассылки от указанной организации",
		GenitivePlural = "массовых рассылок от указанных организаций",
		Nominative = "массовая рассылка от указанной организации",
		NominativePlural = "массовые рассылки от указанных организаций",
		Prepositional = "массовой рассылке от указанной организации",
		PrepositionalPlural = "массовых рассылках от указанных организаций")]
	public class OrganizationBulkEmail : CounterpartyEmail
	{
		private Organization _organization;
		/// <inheritdoc />
		public override IEmailableDocument EmailableDocument { get; }

		/// <inheritdoc />
		public override CounterpartyEmailType Type => CounterpartyEmailType.OrganizationBulk;

		/// <summary>
		/// Организация, от которой будет отправлено письмо
		/// </summary>
		[Display(Name = "Организация")]
		public Organization Organnization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}
	}
}
