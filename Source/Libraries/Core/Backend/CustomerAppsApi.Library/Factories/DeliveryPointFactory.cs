using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using VodovozBusiness.Services.Clients.DeliveryPoints;

namespace CustomerAppsApi.Library.Factories
{
	public class DeliveryPointFactory : IDeliveryPointFactory
	{
		private readonly IDeliveryPointBuildingNumberHandler _deliveryPointBuildingNumberHandler;

		public DeliveryPointFactory(
			IDeliveryPointBuildingNumberHandler deliveryPointBuildingNumberHandler)
		{
			_deliveryPointBuildingNumberHandler =
				deliveryPointBuildingNumberHandler ?? throw new ArgumentNullException(nameof(deliveryPointBuildingNumberHandler));
		}
		
		public ExternalCreatingDeliveryPoint CreateNewExternalCreatingDeliveryPoint(Source source, string uniqueKey)
		{
			return new ExternalCreatingDeliveryPoint
			{
				Source = (int)source,
				UniqueKey = uniqueKey,
				CreatingDate = DateTime.Today
			};
		}
		
		public DeliveryPoint CreateNewDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			var formattedBuilding = _deliveryPointBuildingNumberHandler.TryConvertBuildingStringToErpFormat(
				newDeliveryPointInfoDto.Building);
			
			return new DeliveryPoint
			{
				Counterparty = new Counterparty
				{
					Id = newDeliveryPointInfoDto.CounterpartyErpId
				},
				Category = new DeliveryPointCategory
				{
					Id = newDeliveryPointInfoDto.DeliveryPointCategoryId
				},
				City = newDeliveryPointInfoDto.City,
				LocalityType = newDeliveryPointInfoDto.LocalityType,
				LocalityTypeShort = newDeliveryPointInfoDto.LocalityTypeShort,
				Street = newDeliveryPointInfoDto.Street.Trim(' '),
				StreetType = newDeliveryPointInfoDto.StreetType,
				StreetTypeShort = newDeliveryPointInfoDto.StreetTypeShort,
				Building = formattedBuilding,
				Floor = newDeliveryPointInfoDto.Floor,
				Entrance = newDeliveryPointInfoDto.Entrance,
				Room = newDeliveryPointInfoDto.Room,
				OnlineComment = newDeliveryPointInfoDto.OnlineComment,
				Intercom = newDeliveryPointInfoDto.Intercom,
				CityFiasGuid = newDeliveryPointInfoDto.CityFiasGuid,
				StreetFiasGuid = newDeliveryPointInfoDto.StreetFiasGuid,
				BuildingFiasGuid = newDeliveryPointInfoDto.BuildingFiasGuid,
				BuildingFromOnline = newDeliveryPointInfoDto.Building
			};
		}
		
		public DeliveryPointsDto CreateDeliveryPointsDto(IEnumerable<DeliveryPointForSendNode> deliveryPointsForSend)
		{
			return new DeliveryPointsDto
			{
				DeliveryPointsInfo = deliveryPointsForSend.Select(CreateDeliveryPointDto).ToList()
			};
		}
		
		public DeliveryPointsDto CreateErrorDeliveryPointsInfo(string errorMessage)
		{
			return new DeliveryPointsDto
			{
				ErrorDescription = errorMessage
			};
		}

		public CreatedDeliveryPointDto CreateDeliveryPointDto(NewDeliveryPointInfoDto newDeliveryPointInfoDto, int deliveryPointId)
		{
			return new CreatedDeliveryPointDto
			{
				DeliveryPointErpId = deliveryPointId,
				CounterpartyErpId = newDeliveryPointInfoDto.CounterpartyErpId,
				DeliveryPointCategoryId = newDeliveryPointInfoDto.DeliveryPointCategoryId,
				City = newDeliveryPointInfoDto.City,
				LocalityType = newDeliveryPointInfoDto.LocalityType,
				LocalityTypeShort = newDeliveryPointInfoDto.LocalityTypeShort,
				Street = newDeliveryPointInfoDto.Street,
				StreetType = newDeliveryPointInfoDto.StreetType,
				StreetTypeShort = newDeliveryPointInfoDto.StreetTypeShort,
				Building = newDeliveryPointInfoDto.Building,
				Room = newDeliveryPointInfoDto.Room,
				Floor = newDeliveryPointInfoDto.Floor,
				Entrance = newDeliveryPointInfoDto.Entrance,
				Latitude = newDeliveryPointInfoDto.Latitude,
				Longitude = newDeliveryPointInfoDto.Longitude,
				OnlineComment = newDeliveryPointInfoDto.OnlineComment,
				Intercom = newDeliveryPointInfoDto.Intercom,
				CityFiasGuid = newDeliveryPointInfoDto.CityFiasGuid,
				StreetFiasGuid = newDeliveryPointInfoDto.StreetFiasGuid,
				BuildingFiasGuid = newDeliveryPointInfoDto.BuildingFiasGuid
			};
		}

		private CreatedDeliveryPointDto CreateDeliveryPointDto(DeliveryPointForSendNode deliveryPointForSend)
		{
			return new CreatedDeliveryPointDto
			{
				DeliveryPointErpId = deliveryPointForSend.Id,
				CounterpartyErpId = deliveryPointForSend.CounterpartyId,
				DeliveryPointCategoryId = deliveryPointForSend.CategoryId,
				City = deliveryPointForSend.City,
				LocalityType = deliveryPointForSend.LocalityType,
				LocalityTypeShort = deliveryPointForSend.LocalityTypeShort,
				Street = deliveryPointForSend.Street,
				StreetType = deliveryPointForSend.StreetType,
				StreetTypeShort = deliveryPointForSend.StreetTypeShort,
				Building = deliveryPointForSend.Building,
				Room = deliveryPointForSend.Room,
				Floor = deliveryPointForSend.Floor,
				Entrance = deliveryPointForSend.Entrance,
				Latitude = deliveryPointForSend.Latitude,
				Longitude = deliveryPointForSend.Longitude,
				OnlineComment = deliveryPointForSend.OnlineComment,
				Intercom = deliveryPointForSend.Intercom
			};
		}
	}
}
