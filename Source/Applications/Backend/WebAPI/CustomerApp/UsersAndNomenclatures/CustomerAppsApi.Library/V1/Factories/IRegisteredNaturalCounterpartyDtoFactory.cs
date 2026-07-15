using CustomerAppsApi.Library.V1.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V1.Factories
{
	public interface IRegisteredNaturalCounterpartyDtoFactory
	{
		RegisteredNaturalCounterpartyDto CreateNewRegisteredNaturalCounterpartyDto(ExternalCounterparty externalCounterparty);
	}
}
