using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Core.Data.Counterparties;

namespace CustomerAppsApi.Library.Models
{
	public interface ICounterpartyModel
	{
		CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto);
		CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto);
		CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto);
		Task<CounterpartyBottlesDebtDto> GetCounterpartyBottlesDebt(int counterpartyId);
		IEnumerable<LegalCounterpartyInfo> GetLegalCustomersByInn(GetLegalCustomersByInnDto dto);
		IEnumerable<LegalCounterpartyInfo> GetLegalCustomers(GetNaturalCounterpartyLegalCustomersDto dto);
		string GetLegalCustomersDtoValidate(GetLegalCustomersByInnDto dto);
		(string Message, RegisteredLegalCustomerDto Data) RegisterLegalCustomer(RegisteringLegalCustomerDto dto);
		string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto);
	}
}
