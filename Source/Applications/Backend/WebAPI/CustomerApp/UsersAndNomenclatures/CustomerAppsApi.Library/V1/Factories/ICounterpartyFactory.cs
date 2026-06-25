using CustomerAppsApi.Library.V1.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V1.Factories
{
	public interface ICounterpartyFactory
	{
		Counterparty CreateCounterpartyFromExternalSource(CounterpartyDto counterpartyDto);
		CounterpartyBottlesDebtDto CounterpartyBottlesDebtDto(int counterpartyId, int debt);
	}
}
