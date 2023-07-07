using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Models
{
	public interface ICounterpartyModel
	{
		CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto);
		CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto);
		CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto);
		Task<CounterpartyBottlesDebtDto> GetCounterpartyBottlesDebt(int counterpartyId);
	}
}
