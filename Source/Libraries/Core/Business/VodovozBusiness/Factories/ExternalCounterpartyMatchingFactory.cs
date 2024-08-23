using System;
using Vodovoz.Core.Domain.Clients;
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
