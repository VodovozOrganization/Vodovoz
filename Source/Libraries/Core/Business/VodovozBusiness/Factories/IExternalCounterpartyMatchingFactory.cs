using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Factories
{
	public interface IExternalCounterpartyMatchingFactory
	{
		ExternalCounterpartyMatching CreateNewExternalCounterpartyMatching(Guid externalCounterpartyId, string phoneNumber,
			CounterpartyFrom counterpartyFrom);
	}
}
