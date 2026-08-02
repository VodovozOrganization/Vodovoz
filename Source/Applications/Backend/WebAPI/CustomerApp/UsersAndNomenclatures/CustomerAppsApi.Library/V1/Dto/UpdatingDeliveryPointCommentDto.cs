using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V1.Dto
{
	public class UpdatingDeliveryPointCommentDto
	{
		public Source Source { get; set; }
		public int DeliveryPointErpId { get; set; }
		public string Comment { get; set; }
	}
}
