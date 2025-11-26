using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Contacts;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.Models
{
	public interface ICounterpartyModel
	{
		CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto);
		CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto);
		CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto);
		CounterpartyBottlesDebtDto GetCounterpartyBottlesDebt(int counterpartyId);
		Task<CounterpartyBottlesDebtDto> GetCounterpartyBottlesDebt(int counterpartyId);
		(string Message, IEnumerable<LegalCounterpartyInfo> Data) GetLegalCustomersByInn(LegalCustomersByInnRequest request);
		(string Message, IEnumerable<LegalCounterpartyInfo> Data) GetNaturalCounterpartyLegalCustomers(
			GetNaturalCounterpartyLegalCustomersDto dto);
		string GetLegalCustomersDtoByInnValidate(LegalCustomersByInnRequest request);
		(string Message, RegisteredLegalCustomerDto Data) RegisterLegalCustomer(RegisteringLegalCustomerDto dto);
		string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto);
		string GetPhonesConnectedToLegalCustomerValidate(GetPhonesConnectedToLegalCustomerDto dto);
		(string Message, PhonesConnectedToLegalCustomerDto Data) GetPhonesConnectedToLegalCustomer(GetPhonesConnectedToLegalCustomerDto dto);
		string UpdateConnectToLegalCustomerByPhone(UpdateConnectToLegalCustomerByPhoneDto dto);
		string UpdateConnectToLegalCustomerByPhoneValidate(UpdateConnectToLegalCustomerByPhoneDto dto);
		string GetNaturalCounterpartyLegalCustomersDtoValidate(GetNaturalCounterpartyLegalCustomersDto dto);
		string LinkingEmailToLegalCounterpartyValidate(LinkingLegalCounterpartyEmailToExternalUser dto);
		Result<CompanyWithActiveEmailResponse> GetCompanyWithActiveEmail(CompanyWithActiveEmailRequest dto);
		Result<string> LinkLegalCounterpartyEmailToExternalUser(LinkingLegalCounterpartyEmailToExternalUser dto);
		Result<CompanyInfoResponse> GetCompanyInfo(CompanyInfoRequest dto);
		Result<LegalCounterpartyContacts> GetLegalCustomerContacts(LegalCounterpartyContactListRequest dto);
		string GetLegalCustomerContactsValidate(LegalCounterpartyContactListRequest dto);
	}
}
