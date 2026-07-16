using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "отправки кодов авторизации",
		Nominative = "отправка кода авторизации")]
	public class AuthorizationCodeEmail : CounterpartyEmail
	{
		private int _externalCounterpartyId;
		
		protected AuthorizationCodeEmail() { }
		
		protected AuthorizationCodeEmail(
			Employee employee,
			Counterparty counterparty,
			int externalCounterpartyId,
			string mailSubject,
			string emailAddress)
		{
			Counterparty = counterparty;
			ExternalCounterpartyId = externalCounterpartyId;
			StoredEmail = new StoredEmail
			{
				State = StoredEmailStates.WaitingToSend,
				Author = employee,
				ManualSending = false,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				Subject = mailSubject,
				RecipientAddress = emailAddress,
				Guid = Guid.NewGuid()
			};
		}

		/// <summary>
		/// Id пользователя ИПЗ <see cref="ExternalCounterparty"/>
		/// </summary>
		public virtual int ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}
		
		public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.AuthorizationCode;

		public static AuthorizationCodeEmail Create(
			Employee employee,
			Counterparty counterparty,
			int externalCounterpartyId,
			string mailSubject,
			string emailAddress)
		{
			return new AuthorizationCodeEmail(employee, counterparty, externalCounterpartyId, mailSubject, emailAddress);
		}
	}
}
