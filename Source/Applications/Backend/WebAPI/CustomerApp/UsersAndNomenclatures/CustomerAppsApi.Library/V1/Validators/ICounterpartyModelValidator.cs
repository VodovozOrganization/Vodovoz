using CustomerAppsApi.Library.V1.Dto;
using CustomerAppsApi.Library.V1.Dto.Counterparties;

namespace CustomerAppsApi.Library.V1.Validators
{
	public interface ICounterpartyModelValidator
	{
		string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto);
		string CounterpartyDtoValidate(CounterpartyDto counterpartyDto);
		string SendingCodeToEmailDtoValidate(SendingCodeToEmailDto codeToEmailDto);
	}
}
