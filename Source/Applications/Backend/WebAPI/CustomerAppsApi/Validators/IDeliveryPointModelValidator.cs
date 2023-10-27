using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Validators
{
	public interface IDeliveryPointModelValidator
	{
		string NewDeliveryPointInfoDtoValidate(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
	}
}
