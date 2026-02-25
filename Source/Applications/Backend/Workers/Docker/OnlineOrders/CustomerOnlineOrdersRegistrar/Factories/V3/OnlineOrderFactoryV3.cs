using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using CustomerOrdersApi.Library.Default.Dto.Orders.OrderItem;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Controllers;

namespace CustomerOnlineOrdersRegistrar.Factories.V3
{
	public class OnlineOrderFactoryV3 : IOnlineOrderFactoryV3
	{
		private readonly IDiscountController _discountController;

		public OnlineOrderFactoryV3(IDiscountController discountController)
		{
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
		}
		
		public OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow,
			OnlineOrderInfoDto orderInfoDto,
			int fastDeliveryScheduleId,
			int selfDeliveryDiscountReasonId)
		{
			var onlineOrder = new OnlineOrder
			{
				Source = orderInfoDto.Source,
				CounterpartyId = orderInfoDto.CounterpartyErpId,
				ExternalCounterpartyId = orderInfoDto.ExternalCounterpartyId,
				ExternalOrderId = orderInfoDto.ExternalOrderId,
				DeliveryPointId = orderInfoDto.DeliveryPointId,
				DeliveryDate = orderInfoDto.DeliveryDate,
				CallBeforeArrivalMinutes = orderInfoDto.CallBeforeArrivalMinutes,
				SelfDeliveryGeoGroupId = orderInfoDto.SelfDeliveryGeoGroupId,
				IsSelfDelivery = orderInfoDto.IsSelfDelivery,
				IsFastDelivery = orderInfoDto.IsFastDelivery,
				IsNeedConfirmationByCall = orderInfoDto.IsNeedConfirmationByCall,
				BottlesReturn = orderInfoDto.BottlesReturn,
				Trifle = orderInfoDto.Trifle,
				ContactPhone = orderInfoDto.ContactPhone,
				OnlineOrderPaymentType = orderInfoDto.OnlineOrderPaymentType,
				OnlineOrderStatus = OnlineOrderStatus.New,
				OnlineOrderPaymentStatus = orderInfoDto.OnlineOrderPaymentStatus,
				OnlinePaymentSource = orderInfoDto.OnlinePaymentSource,
				OnlinePayment = orderInfoDto.OnlinePayment,
				OnlineOrderSum = orderInfoDto.OrderSum,
				DontArriveBeforeInterval = orderInfoDto.DontArriveBeforeInterval
			};

			onlineOrder.DeliveryScheduleId = onlineOrder.IsFastDelivery ? fastDeliveryScheduleId : orderInfoDto.DeliveryScheduleId;

			if(!onlineOrder.IsFastDelivery && onlineOrder.DeliveryScheduleId == fastDeliveryScheduleId)
			{
				onlineOrder.IsFastDelivery = true;
			}

			UpdateOnlineComment(onlineOrder, orderInfoDto.OnlineOrderComment);
			InitializeOnlineOrderReferences(uow, onlineOrder, orderInfoDto);
			AddOrderItems(uow, onlineOrder, selfDeliveryDiscountReasonId, orderInfoDto.OnlineOrderItems);
			AddRentPackages(uow, onlineOrder, orderInfoDto.OnlineRentPackages);
			onlineOrder.Created = DateTime.Now;

			return onlineOrder;
		}

		private void UpdateOnlineComment(OnlineOrder onlineOrder, string onlineOrderComment)
		{
			if(!string.IsNullOrWhiteSpace(onlineOrderComment)
				&& onlineOrderComment.Length > OnlineOrder.CommentMaxLength)
			{
				const int maxLength = OnlineOrder.CommentMaxLength - 3;
				onlineOrderComment = onlineOrderComment[..maxLength] + "...";
			}
			
			onlineOrder.OnlineOrderComment = onlineOrderComment;
		}

		private void AddOrderItems(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			int selfDeliveryDiscountReasonId,
			IEnumerable<OnlineOrderItemDto> onlineOrderItemsDtos)
		{
			if(onlineOrderItemsDtos is null)
			{
				return;
			}

			foreach(var onlineOrderItemDto in onlineOrderItemsDtos)
			{
				var nomenclature = uow.GetById<Nomenclature>(onlineOrderItemDto.NomenclatureId);

				DiscountReason applicableDiscountReason = null;
				
				if(onlineOrderItemDto.DiscountReasonId.HasValue)
				{
					applicableDiscountReason = uow.GetById<DiscountReason>(onlineOrderItemDto.DiscountReasonId.Value);
				}
				else if(onlineOrder.IsSelfDelivery
				        && !onlineOrderItemDto.PromoSetId.HasValue
				        && nomenclature != null)
				{
					var discountReason = uow.GetById<DiscountReason>(selfDeliveryDiscountReasonId);

					if(_discountController.IsApplicableDiscount(discountReason, nomenclature))
					{
						applicableDiscountReason = discountReason;
					}
				}
				
				PromotionalSet promoSet = null;

				if(onlineOrderItemDto.PromoSetId.HasValue)
				{
					promoSet = uow.GetById<PromotionalSet>(onlineOrderItemDto.PromoSetId.Value);
				}
				
				var onlineOrderItem = OnlineOrderItem.Create(
					onlineOrderItemDto.NomenclatureId,
					onlineOrderItemDto.Count,
					onlineOrderItemDto.IsDiscountInMoney,
					onlineOrderItemDto.IsFixedPrice,
					onlineOrderItemDto.Discount,
					onlineOrderItemDto.Price,
					onlineOrderItemDto.PromoSetId,
					applicableDiscountReason,
					nomenclature,
					promoSet,
					onlineOrder);

				onlineOrder.OnlineOrderItems.Add(onlineOrderItem);
			}
		}

		private void AddRentPackages(IUnitOfWork uow, OnlineOrder onlineOrder, IList<OnlineRentPackageDto> onlineRentPackagesDtos)
		{
			if(onlineRentPackagesDtos is null)
			{
				return;
			}

			foreach(var onlineRentPackageDto in onlineRentPackagesDtos)
			{
				var rentPackage = uow.GetById<FreeRentPackage>(onlineRentPackageDto.RentPackageId);
				
				var onlineRentPackage = OnlineFreeRentPackage.Create(
					onlineRentPackageDto.RentPackageId,
					onlineRentPackageDto.Count,
					onlineRentPackageDto.Price,
					rentPackage,
					onlineOrder);
					
				onlineOrder.OnlineRentPackages.Add(onlineRentPackage);
			}
		}
		
		private void InitializeOnlineOrderReferences(IUnitOfWork uow, OnlineOrder onlineOrder, OnlineOrderInfoDto orderInfoDto)
		{
			if(orderInfoDto.CounterpartyErpId.HasValue)
			{
				onlineOrder.Counterparty = uow.GetById<Counterparty>(orderInfoDto.CounterpartyErpId.Value);
			}
			
			if(orderInfoDto.DeliveryPointId.HasValue)
			{
				onlineOrder.DeliveryPoint = uow.GetById<DeliveryPoint>(orderInfoDto.DeliveryPointId.Value);
			}
			
			if(orderInfoDto.DeliveryScheduleId.HasValue)
			{
				onlineOrder.DeliverySchedule = uow.GetById<DeliverySchedule>(orderInfoDto.DeliveryScheduleId.Value);
			}
			
			if(orderInfoDto.SelfDeliveryGeoGroupId.HasValue)
			{
				onlineOrder.SelfDeliveryGeoGroup = uow.GetById<GeoGroup>(orderInfoDto.SelfDeliveryGeoGroupId.Value);
			}
		}
	}
}
