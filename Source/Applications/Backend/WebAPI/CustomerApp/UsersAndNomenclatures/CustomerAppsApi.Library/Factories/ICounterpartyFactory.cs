using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Factories
{
	public interface ICounterpartyFactory
	{
		Counterparty CreateCounterpartyFromExternalSource(CounterpartyDto counterpartyDto);
		CounterpartyBottlesDebtDto CounterpartyBottlesDebtDto(int counterpartyId, int debt);
	}
}
