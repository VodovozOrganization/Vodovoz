using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;

namespace CustomerAppsApi.Library.Models
{
	public interface ICounterpartyModel
	{
		CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto);
		CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto, bool isDryRun = false);
		CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto, bool isDryRun  = false);
		CounterpartyBottlesDebtDto GetCounterpartyBottlesDebt(int counterpartyId);
	}
}
