using System.Collections.Generic;

namespace CustomerAppsApi.Library.V1.Dto
{
	public class DeliveryPointsDto
	{
		public string ErrorDescription { get; set; }
		public IList<CreatedDeliveryPointDto> DeliveryPointsInfo { get; set; }
	}
}
