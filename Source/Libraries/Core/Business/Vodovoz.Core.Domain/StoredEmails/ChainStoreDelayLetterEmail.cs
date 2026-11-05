using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Напоминание об оплате заказа сети или тендерного клиента
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Neuter,
		Accusative = "напоминание об оплате",
		AccusativePlural = "напоминания об оплате",
		Genitive = "напоминания об оплате",
		GenitivePlural = "напоминаний об оплате",
		Nominative = "напоминание об оплате",
		NominativePlural = "напоминания об оплате",
		Prepositional = "напоминании об оплате",
		PrepositionalPlural = "напоминаниях об оплате")]
	public class ChainStoreDelayLetterEmail : CounterpartyEmail
	{
		public override IEmailableDocument EmailableDocument { get; }
		public override CounterpartyEmailType Type => CounterpartyEmailType.ChainStoreDelayLetter;
	}
}
