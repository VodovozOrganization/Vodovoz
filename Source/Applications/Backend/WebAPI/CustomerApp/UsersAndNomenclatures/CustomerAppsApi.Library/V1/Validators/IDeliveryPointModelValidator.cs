using CustomerAppsApi.Library.V1.Dto;

namespace CustomerAppsApi.Library.V1.Validators
{
	public interface IDeliveryPointModelValidator
	{
		string NewDeliveryPointInfoDtoValidate(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
	}
}
