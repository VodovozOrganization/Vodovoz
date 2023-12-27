using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Factories
{
	public interface IRegisteredNaturalCounterpartyDtoFactory
	{
		RegisteredNaturalCounterpartyDto CreateNewRegisteredNaturalCounterpartyDto(ExternalCounterparty externalCounterparty);
	}
}
