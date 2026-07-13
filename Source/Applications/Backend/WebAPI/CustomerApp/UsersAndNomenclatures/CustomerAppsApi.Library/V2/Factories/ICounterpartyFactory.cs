using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface ICounterpartyFactory
	{
		Counterparty CreateCounterpartyFromExternalSource(CounterpartyDto counterpartyDto);
		CounterpartyBottlesDebtDto CounterpartyBottlesDebtDto(int counterpartyId, int debt);
	}
}
