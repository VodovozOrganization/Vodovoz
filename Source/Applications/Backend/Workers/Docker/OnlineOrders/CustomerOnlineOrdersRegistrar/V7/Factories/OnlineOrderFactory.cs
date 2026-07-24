using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V7.Factories
{
	public class OnlineOrderFactory : IOnlineOrderFactory
	{
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IGenericRepository<FreeRentPackage> _freeRentPackageRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;

		public OnlineOrderFactory(
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IGenericRepository<FreeRentPackage> freeRentPackageRepository,
			IGenericRepository<DiscountReason> discountReasonRepository
			)
		{
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_freeRentPackageRepository = freeRentPackageRepository ?? throw new ArgumentNullException(nameof(freeRentPackageRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
		}
		
		public OnlineOrderV2 CreateOnlineOrder(
			IUnitOfWork uow,
			ICreatingOnlineOrder creatingOnlineOrder,
			int fastDeliveryScheduleId
			)
		{
			var onlineOrder = new OnlineOrderV2
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

			UpdateOnlineComment(onlineOrder, creatingOnlineOrder.OnlineOrderComment);
			InitializeOnlineOrderReferences(uow, onlineOrder, creatingOnlineOrder);
			CreateAndAddItems(uow, onlineOrder, creatingOnlineOrder.OnlineOrderItems);
			onlineOrder.Created = DateTime.Now;

			return onlineOrder;
		}

		private void UpdateOnlineComment(OnlineOrderV2 onlineOrder, string onlineOrderComment)
		{
			if(!string.IsNullOrWhiteSpace(onlineOrderComment)
				&& onlineOrderComment.Length > OnlineOrder.CommentMaxLength)
			{
				const int maxLength = OnlineOrder.CommentMaxLength - 3;
				onlineOrderComment = onlineOrderComment[..maxLength] + "...";
			}
			
			onlineOrder.OnlineOrderComment = onlineOrderComment;
		}

		private void CreateAndAddItems(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			IList<OnlineOrderItemDto> onlineOrderItems
			)
		{
			foreach(var onlineOrderItem in onlineOrderItems)
			{
				switch(onlineOrderItem.ItemType)
				{
					case SaleItemType.PromoSet:
						CreateAndAddPromoSetItem(uow, onlineOrder, onlineOrderItem);
						break;
					case SaleItemType.RentPackage:
						CreateAndAddRentPackageItem(uow, onlineOrder, onlineOrderItem);
						break;
					default:
						CreateAndAddOnlineOrderItem(uow, onlineOrder, onlineOrderItem);
						break;
				}
			}
		}

		private void CreateAndAddOnlineOrderItem(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			OnlineOrderItemDto onlineOrderItemDto
			)
		{
			if(onlineOrderItemDto.ItemType is SaleItemType.PromoSet or SaleItemType.RentPackage)
			{
				throw new InvalidOperationException(
					"Нельзя создавать объекты промонабора или пакета аренды в методе для номенклатур!" +
					" Используйте методы CreateAndAddPromoSetItem или CreateAndAddRentPackageItem");
			}
			
			var nomenclature = _nomenclatureRepository
				.GetFirstOrDefault(uow, x => x.Id == onlineOrderItemDto.ErpId);
			
			IList<DiscountReason> discounts = null;

			if(onlineOrderItemDto.DiscountIds.Any())
			{
				discounts = _discountReasonRepository
					.Get(uow, x => onlineOrderItemDto.DiscountIds.Contains(x.Id))
					.ToList();
			}
			
			var onlineOrderItem = OnlineOrderItem.Create(
				onlineOrderItemDto.ErpId,
				onlineOrderItemDto.Count,
				onlineOrderItemDto.IsFixedPrice,
				onlineOrderItemDto.Price,
				onlineOrderItemDto.CurrentSum,
				discounts,
				nomenclature,
				onlineOrder
				);

			onlineOrder.OnlineOrderItems.Add(onlineOrderItem);
		}

		private void CreateAndAddPromoSetItem(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			OnlineOrderItemDto onlineOrderItemDto
			)
		{
			if(onlineOrderItemDto.ItemType != SaleItemType.PromoSet)
			{
				throw new InvalidOperationException(
					"Нельзя создавать объекты не промонабора!" +
					" Используйте методы CreateAndAddOnlineOrderItem или CreateAndAddRentPackageItem");
			}

			var promoSet = _promotionalSetRepository
				.GetFirstOrDefault(uow, x => x.Id == onlineOrderItemDto.ErpId);

			IList<DiscountReason> discounts = null;

			if(onlineOrderItemDto.DiscountIds.Any())
			{
				discounts = _discountReasonRepository
					.Get(uow, x => onlineOrderItemDto.DiscountIds.Contains(x.Id))
					.ToList();
			}
			
			var onlinePromoSet = OnlineOrderPromoSet.Create(
				onlineOrderItemDto.ErpId,
				onlineOrderItemDto.Count,
				onlineOrderItemDto.Price,
				onlineOrder,
				promoSet,
				discounts
				);
			
			onlineOrder.PromoSets.Add(onlinePromoSet);
		}

		private void CreateAndAddRentPackageItem(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			OnlineOrderItemDto onlineOrderItemDto
			)
		{
			if(onlineOrderItemDto.ItemType != SaleItemType.RentPackage)
			{
				throw new InvalidOperationException(
					"Нельзя создавать объекты не пакета аренды!" +
					" Используйте методы CreateAndAddOnlineOrderItem или CreateAndAddPromoSetItem");
			}
			
			var rentPackage = _freeRentPackageRepository
				.GetFirstOrDefault(uow, x => x.Id == onlineOrderItemDto.ErpId);
			
			var onlineRentPackage = OnlineFreeRentPackage.Create(
				onlineOrderItemDto.ErpId,
				(int)onlineOrderItemDto.Count,
				onlineOrderItemDto.Price,
				rentPackage,
				onlineOrder
				);
				
			onlineOrder.OnlineRentPackages.Add(onlineRentPackage);
		}
		
		private void InitializeOnlineOrderReferences(IUnitOfWork uow, OnlineOrderV2 onlineOrder, ICreatingOnlineOrder creatingOnlineOrder)
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
