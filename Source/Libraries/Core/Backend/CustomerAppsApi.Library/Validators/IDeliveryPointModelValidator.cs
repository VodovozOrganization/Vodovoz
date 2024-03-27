using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Library.Validators
{
	public interface IDeliveryPointModelValidator
	{
		string NewDeliveryPointInfoDtoValidate(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
	}
}
