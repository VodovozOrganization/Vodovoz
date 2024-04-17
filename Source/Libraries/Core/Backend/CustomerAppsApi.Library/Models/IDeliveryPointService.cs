using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Library.Models
{
	public interface IDeliveryPointService
	{
		DeliveryPointsDto GetDeliveryPoints(Source source, int counterpartyErpId);
		DeliveryPointDto AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto, out int statusCode);
		int UpdateDeliveryPointOnlineComment(UpdatingDeliveryPointCommentDto updatingComments);
	}
}
