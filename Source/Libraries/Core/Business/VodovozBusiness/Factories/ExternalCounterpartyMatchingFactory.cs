using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Factories
{
	public class ExternalCounterpartyMatchingFactory : IExternalCounterpartyMatchingFactory
	{
		public ExternalCounterpartyMatching CreateNewExternalCounterpartyMatching(Guid externalCounterpartyId, string phoneNumber,
			CounterpartyFrom counterpartyFrom)
		{
			return new ExternalCounterpartyMatching
			{
				PhoneNumber = phoneNumber,
				CounterpartyFrom = counterpartyFrom,
				ExternalCounterpartyGuid = externalCounterpartyId,
				Status = ExternalCounterpartyMatchingStatus.AwaitingProcessing
			};
		}
	}
}
