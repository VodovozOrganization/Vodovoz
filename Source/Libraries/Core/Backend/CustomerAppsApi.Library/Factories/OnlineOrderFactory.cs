using System.Collections.Generic;
using CustomerAppsApi.Library.Dto.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerAppsApi.Library.Factories
{
	public class OnlineOrderFactory : IOnlineOrderFactory
	{
		public OnlineOrder CreateOnlineOrder(OnlineOrderInfoDto orderInfoDto)
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

			AddOrderItems(onlineOrder, orderInfoDto.OnlineOrderItems);
			AddRentPackages(onlineOrder, orderInfoDto.OnlineRentPackages);

			return onlineOrder;
		}

		private void AddOrderItems(OnlineOrder onlineOrder, IEnumerable<OnlineOrderItemDto> onlineOrderItemsDtos)
		{
			if(onlineOrderItemsDtos is null)
			{
				return;
			}

			foreach(var onlineOrderItemDto in onlineOrderItemsDtos)
			{
				var onlineOrderItem = new OnlineOrderItem(
					onlineOrderItemDto.NomenclatureId,
					onlineOrderItemDto.Count,
					onlineOrderItemDto.IsDiscountInMoney,
					onlineOrderItemDto.Discount,
					onlineOrderItemDto.Price,
					onlineOrderItemDto.PromoSetId,
					onlineOrder);
					
				onlineOrder.OnlineOrderItems.Add(onlineOrderItem);
			}
		}
		
		private void AddRentPackages(OnlineOrder onlineOrder, IList<OnlineRentPackageDto> onlineRentPackagesDtos)
		{
			if(onlineRentPackagesDtos is null)
			{
				return;
			}

			foreach(var onlineRentPackageDto in onlineRentPackagesDtos)
			{
				var onlineRentPackage = new OnlineRentPackage
				{
					RentPackageId = onlineRentPackageDto.RentPackageId,
					Count = onlineRentPackageDto.Count,
					Price = onlineRentPackageDto.Price,
					OnlineOrder = onlineOrder
				};
					
				onlineOrder.OnlineRentPackages.Add(onlineRentPackage);
			}
		}
	}
}
