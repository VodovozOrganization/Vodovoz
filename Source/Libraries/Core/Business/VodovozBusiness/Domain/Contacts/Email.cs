using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Contacts
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "E-mail адреса",
		Nominative = "E-mail адрес")]
	[HistoryTrace]
	public class Email : EmailEntity
	{
		private EmailType _emailType;
		private Counterparty _counterparty;

		[Display(Name = "Тип адреса")]
		public virtual new EmailType EmailType
		{
			get => _emailType;
			set => SetField(ref _emailType, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		public static Email Create(string address, Counterparty counterparty, EmailType emailType) =>
			new Email
			{
				Address = address,
				Counterparty = counterparty,
				EmailType = emailType
			};
	}
}
