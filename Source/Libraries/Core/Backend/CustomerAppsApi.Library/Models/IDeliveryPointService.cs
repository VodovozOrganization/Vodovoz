using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Models
{
	public interface IDeliveryPointService
	{
		DeliveryPointsDto GetDeliveryPoints(Source source, int counterpartyErpId);
		CreatedDeliveryPointDto AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto, out int statusCode);
		int UpdateDeliveryPointOnlineComment(UpdatingDeliveryPointCommentDto updatingComments);
	}
}
