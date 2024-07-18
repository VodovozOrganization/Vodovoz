using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Пользователи сайта",
		Nominative = "Пользователь сайта"
	)]
	public class WebSiteCounterparty : ExternalCounterparty
	{
		public WebSiteCounterparty() { }
		
		protected WebSiteCounterparty(Guid externalCounterpartyId, Phone phone, Email email)
		{
			ExternalCounterpartyId = externalCounterpartyId;
			Phone = phone;
			Email = email;
		}
		
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.WebSite;
		
		public static WebSiteCounterparty Create(Guid externalCounterpartyId, Phone phone, Email email)
		{
			return new WebSiteCounterparty(externalCounterpartyId, phone, email);
		}
	}
}
