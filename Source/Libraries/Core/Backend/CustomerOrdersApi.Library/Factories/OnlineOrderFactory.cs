using System.Collections.Generic;
using CustomerOrdersApi.Library.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;

namespace CustomerOrdersApi.Library.Factories
{
	public class OnlineOrderFactory : IOnlineOrderFactory
	{
		public OnlineOrder CreateOnlineOrder(IUnitOfWork uow, OnlineOrderInfoDto orderInfoDto)
		{
			var onlineOrder = new OnlineOrder
			{
				Source = orderInfoDto.Source,
				CounterpartyId = orderInfoDto.CounterpartyErpId,
				ExternalCounterpartyId = orderInfoDto.ExternalCounterpartyId,
				DeliveryPointId = orderInfoDto.DeliveryPointId,
				DeliveryDate = orderInfoDto.DeliveryDate,
				DeliveryScheduleId = orderInfoDto.DeliveryScheduleId,
				SelfDeliveryGeoGroupId = orderInfoDto.SelfDeliveryGeoGroupId,
				IsSelfDelivery = orderInfoDto.IsSelfDelivery,
				IsFastDelivery = orderInfoDto.IsFastDelivery,
				IsNeedConfirmationByCall = orderInfoDto.IsNeedConfirmationByCall,
				BottlesReturn = orderInfoDto.BottlesReturn,
				Trifle = orderInfoDto.Trifle,
				ContactPhone = orderInfoDto.ContactPhone,
				OnlineOrderComment = orderInfoDto.OnlineOrderComment,
				OnlineOrderPaymentType = orderInfoDto.OnlineOrderPaymentType,
				OnlineOrderStatus = OnlineOrderStatus.New,
				OnlineOrderPaymentStatus = orderInfoDto.OnlineOrderPaymentStatus,
				OnlinePaymentSource = orderInfoDto.OnlinePaymentSource,
				OnlinePayment = orderInfoDto.OnlinePayment
			};

			InitializeOnlineOrderReferences(uow, onlineOrder, orderInfoDto);
			AddOrderItems(uow, onlineOrder, orderInfoDto.OnlineOrderItems);
			AddRentPackages(uow, onlineOrder, orderInfoDto.OnlineRentPackages);
			onlineOrder.CalculateSum();

			return onlineOrder;
		}

		private void AddOrderItems(IUnitOfWork uow, OnlineOrder onlineOrder, IEnumerable<OnlineOrderItemDto> onlineOrderItemsDtos)
		{
			if(onlineOrderItemsDtos is null)
			{
				return;
			}

			foreach(var onlineOrderItemDto in onlineOrderItemsDtos)
			{
				var nomenclature = uow.GetById<Nomenclature>(onlineOrderItemDto.NomenclatureId);
				
				PromotionalSet promoSet = null;

				if(onlineOrderItemDto.PromoSetId.HasValue)
				{
					promoSet = uow.GetById<PromotionalSet>(onlineOrderItemDto.PromoSetId.Value);
				}
				
				var onlineOrderItem = OnlineOrderItem.Create(
					onlineOrderItemDto.NomenclatureId,
					onlineOrderItemDto.Count,
					onlineOrderItemDto.IsDiscountInMoney,
					onlineOrderItemDto.Discount,
					onlineOrderItemDto.Price,
					onlineOrderItemDto.PromoSetId,
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
