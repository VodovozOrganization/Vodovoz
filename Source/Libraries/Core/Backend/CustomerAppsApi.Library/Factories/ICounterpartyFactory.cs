using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Factories
{
	public interface ICounterpartyFactory
	{
		Counterparty CreateCounterpartyFromExternalSource(CounterpartyDto counterpartyDto);
		CounterpartyBottlesDebtDto CounterpartyBottlesDebtDto(int counterpartyId, int debt);
	}
}
