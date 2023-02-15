using CustomerAppsApi.Controllers;
using CustomerAppsApi.Dto;

namespace CustomerAppsApi.Models
{
	public interface ICounterpartyModel
	{
		CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto);
		CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto);
		CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto);
	}
}
