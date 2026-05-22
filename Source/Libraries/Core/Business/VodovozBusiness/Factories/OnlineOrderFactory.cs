using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Nodes;

namespace Vodovoz.Factories
{
	public class OnlineOrderFactory : IOnlineOrderFactory
	{
		public OnlineOrder Create(OnlineOrderTemplateData templateData)
		{
			var onlineOrder = new OnlineOrder
			{
				TemplateId = templateData.Id,
				Source = templateData.Source,
				CounterpartyId = templateData.Counterparty.Id,
				Counterparty = templateData.Counterparty,
				ExternalCounterpartyId = templateData.ExternalCounterpartyId,
				DeliveryPointId = templateData.DeliveryPoint.Id,
				DeliveryPoint = templateData.DeliveryPoint,
				DeliveryDate = templateData.DeliveryDate,
				DeliveryScheduleId = templateData.DeliverySchedule.Id,
				DeliverySchedule = templateData.DeliverySchedule,
				CallBeforeArrivalMinutes = templateData.CallBeforeArrivalMinutes,
				SelfDeliveryGeoGroupId = templateData.SelfDeliveryGeoGroup?.Id,
				SelfDeliveryGeoGroup = templateData.SelfDeliveryGeoGroup,
				IsSelfDelivery = templateData.IsSelfDelivery,
				IsFastDelivery = templateData.IsFastDelivery,
				IsNeedConfirmationByCall = templateData.IsNeedConfirmationByCall,
				BottlesReturn = templateData.BottlesReturn,
				ContactPhone = templateData.ContactPhone,
				OnlineOrderPaymentType = templateData.PaymentType,
				OnlineOrderPaymentStatus = OnlineOrderPaymentStatus.UnPaid,
				OnlinePaymentSource = null,
				OnlinePayment = null,
				OnlineOrderComment = templateData.Comment,
				//OnlineOrderSum = templateData.OrderSum,
				//Trifle = templateData.Trifle,
				DontArriveBeforeInterval = templateData.DontArriveBeforeInterval
			};

			if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.UnPaid
				&& onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline)
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.WaitingForPayment;
			}
			else
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
			}
			
			AddOrderItems(onlineOrder, templateData.TemplateProducts);
			onlineOrder.Created = DateTime.Now;

			return onlineOrder;
		}
		
		private void AddOrderItems(OnlineOrder onlineOrder, IEnumerable<OnlineOrderTemplateProduct> products)
		{
			if(products is null)
			{
				return;
			}

			foreach(var product in products)
			{
				//TODO 5695: Пока делаем лишь одну скидку
				var firstDiscount = product.Discounts.FirstOrDefault()
					?? OnlineOrderTemplateProductDiscount.CreateEmptyDiscount(product);

				var onlineOrderItem = OnlineOrderItem.Create(
					product.Nomenclature.Id,
					product.Count,
					product.IsFixedPrice,
					product.Price,
					product.PromoSet?.Id,
					DiscountData.Create(
						firstDiscount.IsDiscountInMoney,
						firstDiscount.IsDiscountInMoney ? firstDiscount.MoneyDiscount : firstDiscount.PercentDiscount,
						firstDiscount.DiscountReason),
					product.Nomenclature,
					product.PromoSet,
					onlineOrder);

				onlineOrder.OnlineOrderItems.Add(onlineOrderItem);
			}
		}
	}
}
