using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Library.Validators
{
	public interface ICounterpartyModelValidator
	{
		string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto);
		string CounterpartyDtoValidate(CounterpartyDto counterpartyDto);
	}
}
