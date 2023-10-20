using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Models
{
	public interface IDeliveryPointModel
	{
		DeliveryPointsDto GetDeliveryPoints(Source source, int counterpartyErpId);
		AddedDeliveryPointDto AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
		UpdatedDeliveryPointCommentDto UpdateDeliveryPointOnlineComment(UpdatingDeliveryPointCommentDto updatingComments);
	}
}
