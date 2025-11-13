using System;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Factories
{
	public interface ICounterpartyModelFactory
	{
		CounterpartyIdentificationDto CreateErrorCounterpartyIdentificationDto(string error);
		CounterpartyIdentificationDto CreateNotFoundCounterpartyIdentificationDto();
		CounterpartyIdentificationDto CreateNeedManualHandlingCounterpartyIdentificationDto();

		CounterpartyManualHandlingDto CreateNeedManualHandlingCounterpartyDto(
			CounterpartyContactInfoDto counterpartyContactInfoDto, CounterpartyFrom counterpartyFrom);

		CounterpartyIdentificationDto CreateSuccessCounterpartyIdentificationDto(ExternalCounterparty externalCounterparty);
		CounterpartyIdentificationDto CreateRegisteredCounterpartyIdentificationDto(ExternalCounterparty externalCounterparty);
		CounterpartyRegistrationDto CreateErrorCounterpartyRegistrationDto(string error);
		CounterpartyRegistrationDto CreateRegisteredCounterpartyRegistrationDto(int counterpartyId);
		CounterpartyUpdateDto CreateErrorCounterpartyUpdateDto(string error);
		CounterpartyUpdateDto CreateNotFoundCounterpartyUpdateDto();
		ExternalCounterparty CreateExternalCounterparty(CounterpartyFrom counterpartyFrom);
		ExternalCounterparty CopyToOtherExternalCounterparty(ExternalCounterparty copyingCounterparty, Guid externalCounterpartyId);

		ExternalCounterpartyMatching CreateNewExternalCounterpartyMatching(Guid externalCounterpartyId, string phoneNumber,
			CounterpartyFrom counterpartyFrom);
	}
}
