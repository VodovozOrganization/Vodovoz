using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Errors;

namespace CustomerAppsApi.Library.Models
{
	public interface ICounterpartyModel
	{
		CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto);
		CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto);
		CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto);
		Task<CounterpartyBottlesDebtDto> GetCounterpartyBottlesDebt(int counterpartyId);
		(string Message, IEnumerable<LegalCounterpartyInfo> Data) GetLegalCustomersByInn(GetLegalCustomersByInnDto dto);
		(string Message, IEnumerable<LegalCounterpartyInfo> Data) GetNaturalCounterpartyLegalCustomers(
			GetNaturalCounterpartyLegalCustomersDto dto);
		string GetLegalCustomersDtoByInnValidate(GetLegalCustomersByInnDto dto);
		(string Message, RegisteredLegalCustomerDto Data) RegisterLegalCustomer(RegisteringLegalCustomerDto dto);
		string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto);
		string GetPhonesConnectedToLegalCustomerValidate(GetPhonesConnectedToLegalCustomerDto dto);
		(string Message, PhonesConnectedToLegalCustomerDto Data) GetPhonesConnectedToLegalCustomer(GetPhonesConnectedToLegalCustomerDto dto);
		string UpdateConnectToLegalCustomerByPhone(UpdateConnectToLegalCustomerByPhoneDto dto);
		string UpdateConnectToLegalCustomerByPhoneValidate(UpdateConnectToLegalCustomerByPhoneDto dto);
		string GetNaturalCounterpartyLegalCustomersDtoValidate(GetNaturalCounterpartyLegalCustomersDto dto);
		string LinkingEmailToLegalCounterpartyValidate(LinkingLegalCounterpartyEmailToExternalUser dto);
		Result<string> GetCompanyWithActiveEmail(CompanyWithActiveEmailRequest dto);
		Result<string> LinkLegalCounterpartyEmailToExternalUser(LinkingLegalCounterpartyEmailToExternalUser dto);
	}
}
