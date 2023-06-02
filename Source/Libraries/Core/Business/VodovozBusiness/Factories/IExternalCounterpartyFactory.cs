using Vodovoz.Domain.Client;

namespace Vodovoz.Factories
{
	public interface IExternalCounterpartyFactory
	{
		ExternalCounterparty CreateNewExternalCounterparty(CounterpartyFrom counterpartyFrom);
	}
}
