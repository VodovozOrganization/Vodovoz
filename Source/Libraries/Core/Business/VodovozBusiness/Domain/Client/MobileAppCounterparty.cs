using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Пользователи мобильного приложения",
		Nominative = "Пользователь мобильного пиложения"
	)]
	public class MobileAppCounterparty : ExternalCounterparty
	{
		public MobileAppCounterparty() { }
		
		protected MobileAppCounterparty(Guid externalCounterpartyId, Phone phone, Email email)
		{
			ExternalCounterpartyId = externalCounterpartyId;
			Phone = phone;
			Email = email;
		}
		
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.MobileApp;

		public static MobileAppCounterparty Create(Guid externalCounterpartyId, Phone phone, Email email)
		{
			return new MobileAppCounterparty(externalCounterpartyId, phone, email);
		}
	}
}
