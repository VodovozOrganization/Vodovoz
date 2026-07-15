using CustomerAppsApi.Library.V1.Dto;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface IDeliveryPointService
	{
		DeliveryPointsDto GetDeliveryPoints(Source source, int counterpartyErpId);
		CreatedDeliveryPointDto AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto, out int statusCode, bool isDryRun = false);
		int UpdateDeliveryPointOnlineComment(UpdatingDeliveryPointCommentDto updatingComments, bool isDryRun = false);
	}
}
