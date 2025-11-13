using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;

namespace CustomerAppsApi.Library.Validators
{
	public interface ICounterpartyModelValidator
	{
		string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto);
		string CounterpartyDtoValidate(CounterpartyDto counterpartyDto);
		string GetLegalCustomersByInnDtoValidate(GetLegalCustomersByInnDto counterpartyDto);
		string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto);
		string ConnectingLegalCustomerValidate(ConnectingLegalCustomerDto dto);
		string GetPhonesConnectedToLegalCustomerValidate(GetPhonesConnectedToLegalCustomerDto dto);
		string UpdateConnectToLegalCustomerByPhoneValidate(UpdateConnectToLegalCustomerByPhoneDto dto);
		string ConnectingNewPhoneToLegalCustomerValidate(ConnectingNewPhoneToLegalCustomerDto dto);
		string GetNaturalCounterpartyLegalCustomersDtoValidate(GetNaturalCounterpartyLegalCustomersDto dto);
		string CompanyWithActiveEmailRequestDataValidate(CompanyWithActiveEmailRequest dto);
		string LinkingEmailToLegalCounterpartyValidate(LinkingLegalCounterpartyEmailToExternalUser dto);
	}
}
