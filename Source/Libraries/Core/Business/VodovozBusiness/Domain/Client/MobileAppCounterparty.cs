using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Пользователи мобильного приложения",
		Nominative = "Пользователь мобильного приложения")]
	public class MobileAppCounterparty : ExternalCounterparty
	{
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.MobileApp;
	}
}
