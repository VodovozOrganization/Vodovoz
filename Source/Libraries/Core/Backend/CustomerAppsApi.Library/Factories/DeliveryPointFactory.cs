using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Factories
{
	public class DeliveryPointFactory : IDeliveryPointFactory
	{
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
				Street = newDeliveryPointInfoDto.Street,
				StreetType = newDeliveryPointInfoDto.StreetType,
				StreetTypeShort = newDeliveryPointInfoDto.StreetTypeShort,
				Building = newDeliveryPointInfoDto.Building,
				Floor = newDeliveryPointInfoDto.Floor,
				Entrance = newDeliveryPointInfoDto.Entrance,
				Room = newDeliveryPointInfoDto.Room,
				OnlineComment = newDeliveryPointInfoDto.OnlineComment,
				Intercom = newDeliveryPointInfoDto.Intercom
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

		public DeliveryPointDto CreateDeliveryPointDto(NewDeliveryPointInfoDto newDeliveryPointInfoDto, int deliveryPointId)
		{
			return new DeliveryPointDto
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
				Intercom = newDeliveryPointInfoDto.Intercom
			};
		}

		private DeliveryPointDto CreateDeliveryPointDto(DeliveryPointForSendNode deliveryPointForSend)
		{
			return new DeliveryPointDto
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
