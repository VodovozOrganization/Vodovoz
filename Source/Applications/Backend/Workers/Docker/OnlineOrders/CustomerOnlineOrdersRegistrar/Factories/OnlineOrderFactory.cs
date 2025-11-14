using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using CustomerOrdersApi.Library.V4.Dto.Orders.OrderItem;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Controllers;

namespace CustomerOnlineOrdersRegistrar.Factories
{
	public class OnlineOrderFactory : IOnlineOrderFactory
	{
		private readonly IDiscountController _discountController;

		public OnlineOrderFactory(IDiscountController discountController)
		{
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
		}
		
		public OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow,
			ICreatingOnlineOrder creatingOnlineOrder,
			int fastDeliveryScheduleId,
			int selfDeliveryDiscountReasonId)
		{
			var onlineOrder = new OnlineOrder
			{
				Source = creatingOnlineOrder.Source,
				CounterpartyId = creatingOnlineOrder.CounterpartyErpId,
				ExternalCounterpartyId = creatingOnlineOrder.ExternalCounterpartyId,
				ExternalOrderId = creatingOnlineOrder.ExternalOrderId,
				DeliveryPointId = creatingOnlineOrder.DeliveryPointId,
				DeliveryDate = creatingOnlineOrder.DeliveryDate,
				CallBeforeArrivalMinutes = creatingOnlineOrder.CallBeforeArrivalMinutes,
				SelfDeliveryGeoGroupId = creatingOnlineOrder.SelfDeliveryGeoGroupId,
				IsSelfDelivery = creatingOnlineOrder.IsSelfDelivery,
				IsFastDelivery = creatingOnlineOrder.IsFastDelivery,
				IsNeedConfirmationByCall = creatingOnlineOrder.IsNeedConfirmationByCall,
				BottlesReturn = creatingOnlineOrder.BottlesReturn,
				Trifle = creatingOnlineOrder.Trifle,
				ContactPhone = creatingOnlineOrder.ContactPhone,
				OnlineOrderComment = creatingOnlineOrder.OnlineOrderComment,
				OnlineOrderPaymentType = creatingOnlineOrder.OnlineOrderPaymentType,
				OnlineOrderPaymentStatus = creatingOnlineOrder.OnlineOrderPaymentStatus,
				OnlinePaymentSource = creatingOnlineOrder.OnlinePaymentSource,
				OnlinePayment = creatingOnlineOrder.OnlinePayment,
				OnlineOrderSum = creatingOnlineOrder.OrderSum,
				DontArriveBeforeInterval = creatingOnlineOrder.DontArriveBeforeInterval
			};

			onlineOrder.DeliveryScheduleId = onlineOrder.IsFastDelivery ? fastDeliveryScheduleId : creatingOnlineOrder.DeliveryScheduleId;

			if(!onlineOrder.IsFastDelivery && onlineOrder.DeliveryScheduleId == fastDeliveryScheduleId)
			{
				onlineOrder.IsFastDelivery = true;
			}

			if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.UnPaid
				&& onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline)
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.WaitingForPayment;
			}
			else
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
			}

			InitializeOnlineOrderReferences(uow, onlineOrder, creatingOnlineOrder);
			AddOrderItems(uow, onlineOrder, selfDeliveryDiscountReasonId, creatingOnlineOrder.OnlineOrderItems);
			AddRentPackages(uow, onlineOrder, creatingOnlineOrder.OnlineRentPackages);
			onlineOrder.Created = DateTime.Now;

			return onlineOrder;
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
		
		private void InitializeOnlineOrderReferences(IUnitOfWork uow, OnlineOrder onlineOrder, ICreatingOnlineOrder creatingOnlineOrder)
		{
			if(creatingOnlineOrder.CounterpartyErpId.HasValue)
			{
				onlineOrder.Counterparty = uow.GetById<Counterparty>(creatingOnlineOrder.CounterpartyErpId.Value);
			}
			
			if(creatingOnlineOrder.DeliveryPointId.HasValue)
			{
				onlineOrder.DeliveryPoint = uow.GetById<DeliveryPoint>(creatingOnlineOrder.DeliveryPointId.Value);
			}
			
			if(creatingOnlineOrder.DeliveryScheduleId.HasValue)
			{
				onlineOrder.DeliverySchedule = uow.GetById<DeliverySchedule>(creatingOnlineOrder.DeliveryScheduleId.Value);
			}
			
			if(creatingOnlineOrder.SelfDeliveryGeoGroupId.HasValue)
			{
				onlineOrder.SelfDeliveryGeoGroup = uow.GetById<GeoGroup>(creatingOnlineOrder.SelfDeliveryGeoGroupId.Value);
			}
		}
	}
}
