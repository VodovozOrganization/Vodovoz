using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface IRegisteredNaturalCounterpartyDtoFactory
	{
		RegisteredNaturalCounterpartyDto CreateNewRegisteredNaturalCounterpartyDto(ExternalCounterparty externalCounterparty);
	}
}
