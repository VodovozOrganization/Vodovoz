using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace CustomerAppsApi.Factories
{
	public class DeliveryPointFactory : IDeliveryPointFactory
	{
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
				Street = newDeliveryPointInfoDto.Street,
				Building = newDeliveryPointInfoDto.Building,
				Floor = newDeliveryPointInfoDto.Floor,
				Entrance = newDeliveryPointInfoDto.Entrance,
				Room = newDeliveryPointInfoDto.Room,
				OnlineComment = newDeliveryPointInfoDto.OnlineComment,
				Intercom = newDeliveryPointInfoDto.Intercom,
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

		public AddedDeliveryPointDto CreateAddedDeliveryPointDto()
		{
			return new AddedDeliveryPointDto
			{
				Status = AddedDeliveryPointStatus.DeliveryPointAdded
			};
		}

		public AddedDeliveryPointDto CreateErrorAddedDeliveryPointDto(string errorMessage)
		{
			return new AddedDeliveryPointDto
			{
				ErrorDescription = errorMessage,
				Status = AddedDeliveryPointStatus.Error
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
				Street = deliveryPointForSend.Street,
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
