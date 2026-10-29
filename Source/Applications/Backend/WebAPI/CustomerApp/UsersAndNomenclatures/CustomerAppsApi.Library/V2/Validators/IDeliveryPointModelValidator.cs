using CustomerAppsApi.Library.V2.Dto;

namespace CustomerAppsApi.Library.V2.Validators
{
	public interface IDeliveryPointModelValidator
	{
		string NewDeliveryPointInfoDtoValidate(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
	}
}
