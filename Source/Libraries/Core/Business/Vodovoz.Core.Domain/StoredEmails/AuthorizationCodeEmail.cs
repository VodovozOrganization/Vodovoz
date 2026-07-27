using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "отправки кодов авторизации",
		Nominative = "отправка кода авторизации")]
	public class AuthorizationCodeEmail : CounterpartyEmail
	{
		private int _externalCounterpartyId;
		
		protected AuthorizationCodeEmail() { }
		
		protected AuthorizationCodeEmail(
			EmployeeEntity employee,
			CounterpartyEntity counterparty,
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
			EmployeeEntity employee,
			CounterpartyEntity counterparty,
			int externalCounterpartyId,
			string mailSubject,
			string emailAddress)
		{
			return new AuthorizationCodeEmail(employee, counterparty, externalCounterpartyId, mailSubject, emailAddress);
		}
	}
}
