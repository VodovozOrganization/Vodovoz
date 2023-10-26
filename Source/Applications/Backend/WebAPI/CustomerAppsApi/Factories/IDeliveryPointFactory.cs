using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Factories
{
	public interface IDeliveryPointFactory
	{
		ExternalCreatingDeliveryPoint CreateNewExternalCreatingDeliveryPoint(Source source, string uniqueKey);
		DeliveryPoint CreateNewDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
		DeliveryPointsDto CreateDeliveryPointsInfo(IEnumerable<DeliveryPointForSendNode> deliveryPointsForSend);
		DeliveryPointsDto CreateErrorDeliveryPointsInfo(string errorMessage);
		UpdatedDeliveryPointCommentDto CreateSuccessUpdatedDeliveryPointCommentsDto();
		UpdatedDeliveryPointCommentDto CreateNotFoundUpdatedDeliveryPointCommentsDto();
		UpdatedDeliveryPointCommentDto CreateErrorUpdatedDeliveryPointCommentsDto(string errorMessage);
	}
}
