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
		
		public DeliveryPointsDto CreateDeliveryPointsInfo(IEnumerable<DeliveryPointForSendNode> deliveryPointsForSend)
		{
			return new DeliveryPointsDto
			{
				DeliveryPointsInfo = deliveryPointsForSend.Select(CreateDeliveryPointInfoDto).ToList()
			};
		}
		
		public DeliveryPointsDto CreateErrorDeliveryPointsInfo(string errorMessage)
		{
			return new DeliveryPointsDto
			{
				ErrorDescription = errorMessage
			};
		}

		public UpdatedDeliveryPointCommentDto CreateSuccessUpdatedDeliveryPointCommentsDto()
		{
			return new UpdatedDeliveryPointCommentDto
			{
				Status = UpdatedDeliveryPointCommentStatus.CommentUpdated
			};
		}
		
		public UpdatedDeliveryPointCommentDto CreateNotFoundUpdatedDeliveryPointCommentsDto()
		{
			return new UpdatedDeliveryPointCommentDto
			{
				Status = UpdatedDeliveryPointCommentStatus.DeliveryPointNotFound
			};
		}
		
		public UpdatedDeliveryPointCommentDto CreateErrorUpdatedDeliveryPointCommentsDto(string errorMessage)
		{
			return new UpdatedDeliveryPointCommentDto
			{
				ErrorDescription = errorMessage,
				Status = UpdatedDeliveryPointCommentStatus.Error
			};
		}

		private DeliveryPointDto CreateDeliveryPointInfoDto(DeliveryPointForSendNode deliveryPointForSend)
		{
			return new DeliveryPointDto
			{
				DeliveryPointErpId = deliveryPointForSend.Id,
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
