using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Library.Factories
{
	public interface IDeliveryPointFactory
	{
		ExternalCreatingDeliveryPoint CreateNewExternalCreatingDeliveryPoint(Source source, string uniqueKey);
		DeliveryPoint CreateNewDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto);
		DeliveryPointsDto CreateDeliveryPointsDto(IEnumerable<DeliveryPointForSendNode> deliveryPointsForSend);
		DeliveryPointsDto CreateErrorDeliveryPointsInfo(string errorMessage);
		CreatedDeliveryPointDto CreateDeliveryPointDto(NewDeliveryPointInfoDto newDeliveryPointInfoDto, int deliveryPointId);
	}
}
