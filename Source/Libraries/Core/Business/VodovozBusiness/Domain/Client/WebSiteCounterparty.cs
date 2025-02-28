using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Пользователи сайта",
		Nominative = "Пользователь сайта")]
	public class WebSiteCounterparty : ExternalCounterparty
	{
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.WebSite;
	}
}
