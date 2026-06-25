using System.Collections.Generic;

namespace CustomerAppsApi.Library.Dto
{
	public class DeliveryPointsDto
	{
		public string ErrorDescription { get; set; }
		public IList<CreatedDeliveryPointDto> DeliveryPointsInfo { get; set; }
	}
}
