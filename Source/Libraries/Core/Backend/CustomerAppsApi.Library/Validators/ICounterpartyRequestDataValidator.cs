using CustomerAppsApi.Library.Dto.Counterparties;

namespace CustomerAppsApi.Library.Validators
{
	public interface ICounterpartyRequestDataValidator
	{
		string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto);
		string CounterpartyDtoValidate(CounterpartyDto counterpartyDto);
		string LegalCustomersByInnValidate(LegalCustomersByInnRequest dto);
		string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto);
		string ConnectingLegalCustomerValidate(ConnectingLegalCustomerDto dto);
		string GetPhonesConnectedToLegalCustomerValidate(GetPhonesConnectedToLegalCustomerDto dto);
		string UpdateConnectToLegalCustomerByPhoneValidate(UpdateConnectToLegalCustomerByPhoneDto dto);
		string ConnectingNewPhoneToLegalCustomerValidate(ConnectingNewPhoneToLegalCustomerDto dto);
		string GetNaturalCounterpartyLegalCustomersDtoValidate(GetNaturalCounterpartyLegalCustomersDto dto);
		string CompanyWithActiveEmailValidate(CompanyWithActiveEmailRequest dto);
		string CompanyInfoRequestDataValidate(CompanyInfoRequest dto);
		string LinkingEmailToLegalCounterpartyValidate(LinkingLegalCounterpartyEmailToExternalUser dto);
		string GetLegalCustomerContactsValidate(LegalCounterpartyContactListRequest dto);
		string CheckPasswordValidate(CheckPasswordRequest dto);
	}
}
