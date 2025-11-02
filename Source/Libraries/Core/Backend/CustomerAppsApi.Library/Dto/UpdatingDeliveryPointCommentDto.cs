using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto
{
	public class UpdatingDeliveryPointCommentDto
	{
		public Source Source { get; set; }
		public int DeliveryPointErpId { get; set; }
		public string Comment { get; set; }
	}
}
