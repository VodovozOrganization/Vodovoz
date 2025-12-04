using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Client
{
	/// <summary>
	/// Клиент пришедший через ИИ бота
	/// </summary>
	public class AiBotCounterparty : ExternalCounterparty
	{
		/// <inheritdoc/>
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.AiBot;
	}
}
