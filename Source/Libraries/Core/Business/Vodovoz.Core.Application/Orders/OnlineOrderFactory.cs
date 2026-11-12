using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Factories;

namespace Vodovoz.Factories
{
	public class OnlineOrderFactory : IOnlineOrderFactory
	{
		public OnlineOrder Create(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			decimal templateSum,
			DateTime deliveryDate,
			IEnumerable<(decimal Sum, OnlineOrderTemplateSaleItem SaleItem)> saleItemsWithSum,
			IEnumerable<OnlineOrderTemplatePromoSet> promoSets,
			IEnumerable<OnlineOrderTemplateFreeRentPackage> freeRentPackages
			)
		{
			var onlineOrder = new OnlineOrderV2
			{
				Template = template,
				Source = template.Source,
				CounterpartyId = template.CounterpartyId,
				ExternalCounterpartyId = template.ExternalCounterpartyId,
				DeliveryPointId = template.DeliveryPointId,
				DeliveryDate = deliveryDate,
				DeliveryScheduleId = template.Schedule.DeliveryScheduleId,
				CallBeforeArrivalMinutes = template.CallBeforeArrivalMinutes,
				SelfDeliveryGeoGroupId = null,
				SelfDeliveryGeoGroup = null,
				IsSelfDelivery = false,
				IsFastDelivery = false,
				IsNeedConfirmationByCall = template.IsNeedConfirmationByCall,
				BottlesReturn = template.BottlesReturn,
				ContactPhone = template.ContactPhone,
				OnlineOrderPaymentType = template.PaymentType,
				OnlineOrderPaymentStatus = OnlineOrderPaymentStatus.UnPaid,
				OnlinePaymentSource = null,
				OnlinePayment = null,
				OnlineOrderComment = template.Comment,
				OnlineOrderSum = templateSum,
				Trifle = null,
				DontArriveBeforeInterval = template.DontArriveBeforeInterval
			};
			
			CreateAndAddItems(uow, onlineOrder, saleItemsWithSum, promoSets, freeRentPackages);
			InitializeOnlineOrderReferences(uow, onlineOrder);
			
			onlineOrder.Created = DateTime.Now;

			return onlineOrder;
		}
		
		private void CreateAndAddItems(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			IEnumerable<(decimal Sum, OnlineOrderTemplateSaleItem SaleItem)> templateSaleItemsWithSum,
			IEnumerable<OnlineOrderTemplatePromoSet> promoSets,
			IEnumerable<OnlineOrderTemplateFreeRentPackage> freeRentPackages
		)
		{
			foreach(var templateSaleItemWithSum in templateSaleItemsWithSum)
			{
				CreateAndAddOnlineOrderItem(uow, onlineOrder, templateSaleItemWithSum);
			}

			foreach(var promoSet in promoSets)
			{
				CreateAndAddPromoSetItem(uow, onlineOrder, promoSet);
			}

			foreach(var freeRentPackage in freeRentPackages)
			{
				CreateAndAddRentPackageItem(uow, onlineOrder, freeRentPackage);
			}
		}
		
		private void CreateAndAddOnlineOrderItem(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			(decimal Sum, OnlineOrderTemplateSaleItem SaleItem) templateSaleItemWithSum
		)
		{
			var saleItem = templateSaleItemWithSum.SaleItem;
			if(saleItem
				.Nomenclature
				.Category is NomenclatureCategory.master)
			{
				onlineOrder.HasService = true;
			}
			
			var onlineOrderItem = OnlineOrderItem.Create(
				saleItem.Nomenclature.Id,
				saleItem.Count,
				saleItem.IsFixedPrice,
				saleItem.Price,
				templateSaleItemWithSum.Sum,
				saleItem.DiscountReasons,
				saleItem.Nomenclature,
				onlineOrder,
				false
			);

			onlineOrder.OnlineOrderItems.Add(onlineOrderItem);
		}
		
		private void CreateAndAddPromoSetItem(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			OnlineOrderTemplatePromoSet templatePromoSet
		)
		{
			var promoSet = templatePromoSet.PromoSet;
			
			//TODO-5696 стоит добавлять персональнуюю скидку? Теперь будут создавать из ДВ и возможно будет персональная скидка у промика
			var onlinePromoSet = OnlineOrderPromoSet.Create(
				promoSet.Id,
				templatePromoSet.Count,
				templatePromoSet.Price,
				onlineOrder,
				promoSet,
				templatePromoSet.DiscountReasons
			);
			
			onlineOrder.PromoSets.Add(onlinePromoSet);
		}
		
		private void CreateAndAddRentPackageItem(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			OnlineOrderTemplateFreeRentPackage templateFreeRentPackage
		)
		{
			var rentPackage = templateFreeRentPackage.FreeRentPackage;
			
			//TODO-5696 Скидки на пакеты аренды?
			var onlineRentPackage = OnlineFreeRentPackage.Create(
				rentPackage.Id,
				templateFreeRentPackage.Count,
				templateFreeRentPackage.Price,
				rentPackage,
				onlineOrder
			);
				
			onlineOrder.OnlineRentPackages.Add(onlineRentPackage);
		}
		
		private void InitializeOnlineOrderReferences(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder)
		{
			if(onlineOrder.CounterpartyId.HasValue)
			{
				onlineOrder.Counterparty = uow.GetById<Counterparty>(onlineOrder.CounterpartyId.Value);
			}
			
			if(onlineOrder.DeliveryPointId.HasValue)
			{
				onlineOrder.DeliveryPoint = uow.GetById<DeliveryPoint>(onlineOrder.DeliveryPointId.Value);
			}
			
			if(onlineOrder.DeliveryScheduleId.HasValue)
			{
				onlineOrder.DeliverySchedule = uow.GetById<DeliverySchedule>(onlineOrder.DeliveryScheduleId.Value);
			}
		}
	}
}
