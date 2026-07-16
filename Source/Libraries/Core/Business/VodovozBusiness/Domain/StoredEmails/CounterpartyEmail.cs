using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "письма контрагенту",
		Nominative = "письмо контрагенту")]
	public abstract class CounterpartyEmail : PropertyChangedBase, IDomainObject
	{
		private StoredEmail _storedEmail;
		private Counterparty _counterparty;
		private int? _organizationId;

		public virtual int Id { get; set; }

		/// <summary>
		/// Данные по отправке
		/// </summary>
		[Display(Name = "Электронная почта")]
		public virtual StoredEmail StoredEmail
		{
			get => _storedEmail;
			set => SetField(ref _storedEmail, value);
		}

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Организация, от которой будет отправлено письмо
		/// </summary>
		[Display(Name = "Организация")]
		public virtual int? OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}

		/// <summary>
		/// Тип почтового отправления
		/// </summary>
		[Display(Name = "Тип почтового отправления")]
		public virtual CounterpartyEmailType Type { get; set; }

		/// <summary>
		/// Отправляемый документ
		/// </summary>
		public abstract IEmailableDocument EmailableDocument { get; }
	}
}
