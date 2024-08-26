using System.Collections.Generic;
using System.Threading.Tasks;
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
		(string Message, IEnumerable<LegalCounterpartyInfo> Data) GetLegalCustomersByInn(GetLegalCustomersByInnDto dto);
		IEnumerable<LegalCounterpartyInfo> GetNaturalCounterpartyLegalCustomers(GetNaturalCounterpartyLegalCustomersDto dto);
		string GetLegalCustomersDtoValidate(GetLegalCustomersByInnDto dto);
		(string Message, RegisteredLegalCustomerDto Data) RegisterLegalCustomer(RegisteringLegalCustomerDto dto);
		(string Message, ConnectedLegalCustomerDto Data) ConnectLegalCustomer(ConnectingLegalCustomerDto dto);
		string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto);
		string ConnectingLegalCustomerValidate(ConnectingLegalCustomerDto dto);
	}
}
