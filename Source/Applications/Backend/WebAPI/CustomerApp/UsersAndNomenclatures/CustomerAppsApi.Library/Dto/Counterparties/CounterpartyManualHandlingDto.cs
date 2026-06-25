using System;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class CounterpartyManualHandlingDto
	{
		public CounterpartyManualHandlingDto(CounterpartyIdentificationDto counterpartyIdentificationDto,
			ExternalCounterpartyMatching externalCounterpartyMatching)
		{
			CounterpartyIdentificationDto =
				counterpartyIdentificationDto ?? throw new ArgumentNullException(nameof(counterpartyIdentificationDto));
			ExternalCounterpartyMatching =
				externalCounterpartyMatching ?? throw new ArgumentNullException(nameof(externalCounterpartyMatching));
		}
		
		public CounterpartyIdentificationDto CounterpartyIdentificationDto { get; }
		public ExternalCounterpartyMatching ExternalCounterpartyMatching { get; }
	}
}
