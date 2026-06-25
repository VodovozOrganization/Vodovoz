using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;

namespace CustomerAppsApi.Library.Validators
{
	public interface ICounterpartyModelValidator
	{
		string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto);
		string CounterpartyDtoValidate(CounterpartyDto counterpartyDto);
		string SendingCodeToEmailDtoValidate(SendingCodeToEmailDto codeToEmailDto);
	}
}
