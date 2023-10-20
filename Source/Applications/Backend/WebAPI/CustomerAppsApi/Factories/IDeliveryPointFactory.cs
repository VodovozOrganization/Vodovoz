using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Factories
{
	public interface IDeliveryPointFactory
	{
		DeliveryPoint CreateNewDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
		DeliveryPointsDto CreateDeliveryPointsInfo(IEnumerable<DeliveryPointForSendNode> deliveryPointsForSend);
		DeliveryPointsDto CreateErrorDeliveryPointsInfo(string errorMessage);
		AddedDeliveryPointDto CreateAddedDeliveryPointDto();
		AddedDeliveryPointDto CreateErrorAddedDeliveryPointDto(string errorMessage);
		UpdatedDeliveryPointCommentDto CreateSuccessUpdatedDeliveryPointCommentsDto();
		UpdatedDeliveryPointCommentDto CreateNotFoundUpdatedDeliveryPointCommentsDto();
		UpdatedDeliveryPointCommentDto CreateErrorUpdatedDeliveryPointCommentsDto(string errorMessage);
	}
}
