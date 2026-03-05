using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderTemplateHandler
	{
		private readonly IGoodsPriceCalculator _priceCalculator;
		private readonly IOnlineOrderRepository _onlineOrderRepository;

		public OnlineOrderTemplateHandler(
			IGoodsPriceCalculator priceCalculator,
			IOnlineOrderRepository onlineOrderRepository)
		{
			_priceCalculator = priceCalculator ?? throw new ArgumentNullException(nameof(priceCalculator));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
		}
		
		public OrderTemplateInfoDto GetFreshOnlineOrderTemplateData(IUnitOfWork uow, int templateId)
		{
			var template = uow.GetById<OnlineOrderTemplate>(templateId);

			if(template is null)
			{
				return null;
			}

			var counterparty = uow.GetById<Counterparty>(template.CounterpartyId);
			var deliveryPoint = uow.GetById<DeliveryPoint>(template.DeliveryPointId);
			var deliverySchedule = uow.GetById<DeliverySchedule>(template.DeliveryScheduleId);
			var items = uow.GetById<OnlineOrderTemplateProduct>(template.TemplateProducts);
			var promoSetsIds = items
				.Where(x => x.PromoSetId.HasValue)
				.ToLookup(x => x.PromoSetId.Value);

			var templateProducts = new List<OrderTemplateProductDto>();
			decimal orderSum = 0;

			var productsWithoutPromoSets = items.Where(x => !x.PromoSetId.HasValue).ToList();

			ProcessItemsWithoutPromoSets(productsWithoutPromoSets, items, deliveryPoint, counterparty, templateProducts, ref orderSum);
			ProcessPromoSets(uow, promoSetsIds, templateProducts, items, deliveryPoint, counterparty, ref orderSum);

			var lastExternalOnlineOrderId = _onlineOrderRepository.GetLastOnlineOrderExternalId(uow, template.CounterpartyId);

			var onlineOrderTemplateInfo = OrderTemplateInfoDto.Create(
				template,
				deliveryPoint.ShortAddress,
				deliverySchedule.DeliveryTime,
				lastExternalOnlineOrderId,
				templateProducts,
				orderSum);

			return onlineOrderTemplateInfo;
		}

		private void ProcessItemsWithoutPromoSets(
			IList<OnlineOrderTemplateProduct> productsWithoutPromoSets,
			IList<OnlineOrderTemplateProduct> items,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IList<OrderTemplateProductDto> templateProducts,
			ref decimal orderSum)
		{
			foreach(var item in productsWithoutPromoSets)
			{
				var price =  _priceCalculator.CalculateItemPrice(
					items,
					deliveryPoint,
					counterparty,
					item,
					false);

				//TODO 5695: нужны ли данные по скидкам для позиций не из промонаборов? Будет ли возможность постоянной скидки, какой-либо для автозаказа
				var onlineOrderItem = OrderTemplateProductDto.Create(
					item.Nomenclature.Id,
					price,
					item.Count,
					item.IsDiscountInMoney,
					item.Discount,
					item.Id,
					item.DiscountReason?.Id);
				
				templateProducts.Add(onlineOrderItem);
				
				orderSum += item.Sum;
			}
		}
		
		private void ProcessPromoSets(
			IUnitOfWork uow,
			ILookup<int, OnlineOrderTemplateProduct> promoSetsIds,
			List<OrderTemplateProductDto> templateProducts,
			IEnumerable<OnlineOrderTemplateProduct> items,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			ref decimal orderSum)
		{
			foreach(var groupingByPromoSetId in promoSetsIds)
			{
				var promoSet = uow.GetById<PromotionalSet>(groupingByPromoSetId.Key);
				decimal promoSetItemsCount = promoSet.PromotionalSetItems.Count;
				decimal providedItemsCount = groupingByPromoSetId.Count();
				var promoSetCount = providedItemsCount / promoSetItemsCount;
				
				if(promoSet.IsArchive)
				{
					//TODO 5695: что делаем, если промонабор заархивировали? Или будем запрещать архивацию если есть хоть один активный автозаказ с ним?
				}

				for(var i = 0; i < promoSetCount; i++)
				{
					foreach(var promoItem in promoSet.PromotionalSetItems)
					{
						var orderTemplateProduct = OrderTemplateProductDto.Create(
							promoItem.Nomenclature.Id,
							_priceCalculator.CalculateItemPrice(
								items,
								deliveryPoint,
								counterparty,
								promoItem,
								false),
							promoItem.Count,
							promoItem.IsDiscountInMoney,
							promoItem.Discount,
							promoSet.Id,
							null);
						
						templateProducts.Add(orderTemplateProduct);
						orderSum += orderTemplateProduct.Sum;
					}
				}
			}
		}
	}
}
