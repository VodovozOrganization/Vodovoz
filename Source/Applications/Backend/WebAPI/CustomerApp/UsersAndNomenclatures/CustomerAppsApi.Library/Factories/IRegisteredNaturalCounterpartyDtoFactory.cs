using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Factories
{
	public interface IRegisteredNaturalCounterpartyDtoFactory
	{
		RegisteredNaturalCounterpartyDto CreateNewRegisteredNaturalCounterpartyDto(ExternalCounterparty externalCounterparty);
	}
}
