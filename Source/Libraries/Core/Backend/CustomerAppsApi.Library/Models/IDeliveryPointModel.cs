using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Models
{
	public interface IDeliveryPointModel
	{
		DeliveryPointsDto GetDeliveryPoints(Source source, int counterpartyErpId);
		DeliveryPointDto AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto, out int statusCode);
		int UpdateDeliveryPointOnlineComment(UpdatingDeliveryPointCommentDto updatingComments);
	}
}
