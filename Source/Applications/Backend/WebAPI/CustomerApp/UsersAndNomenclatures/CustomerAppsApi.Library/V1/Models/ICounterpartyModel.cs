using CustomerAppsApi.Library.V1.Dto.Counterparties;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface ICounterpartyModel
	{
		CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto);
		CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto, bool isDryRun = false);
		CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto, bool isDryRun  = false);
		CounterpartyBottlesDebtDto GetCounterpartyBottlesDebt(int counterpartyId);
	}
}
