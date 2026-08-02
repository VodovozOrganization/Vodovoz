using CustomerAppsApi.Library.V2.Dto;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Validators
{
	public interface ICounterpartyModelValidator
	{
		string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto);
		string CounterpartyDtoValidate(CounterpartyDto counterpartyDto);
		string SendingCodeToEmailDtoValidate(SendingCodeToEmailDto codeToEmailDto);
	}
}
