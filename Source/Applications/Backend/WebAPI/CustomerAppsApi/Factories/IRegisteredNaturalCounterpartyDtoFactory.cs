using CustomerAppsApi.Controllers;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Factories
{
	public interface IRegisteredNaturalCounterpartyDtoFactory
	{
		RegisteredNaturalCounterpartyDto CreateNewRegisteredNaturalCounterpartyDto(ExternalCounterparty externalCounterparty);
	}
}
