using CustomerApps.Contracts.V5;
using NHibernate;
using NHibernate.Multi;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Infrastructure;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Extensions;
using VodovozBusiness.Services.Orders.V5;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderTemplateHandler
	{
		private readonly IGoodsPriceCalculatorV5 _priceCalculator;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOnlineOrderTemplateRepository _onlineOrderTemplateRepository;

		public OnlineOrderTemplateHandler(
			IGoodsPriceCalculatorV5 priceCalculator,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderTemplateRepository onlineOrderTemplateRepository)
		{
			_priceCalculator = priceCalculator ?? throw new ArgumentNullException(nameof(priceCalculator));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_onlineOrderTemplateRepository = onlineOrderTemplateRepository ?? throw new ArgumentNullException(nameof(onlineOrderTemplateRepository));
		}
		
		public async Task<OrderTemplateInfoDto> GetFreshOnlineOrderTemplateDataAsync(IUnitOfWork uow, int templateId)
		{
			const string templateBatchKey = "templateInfo";
			const string counterpartyBatchKey = "counterparty";
			const string deliveryPointBatchKey = "deliveryPoint";
			const string weekdaysBatchKey = "weekdays";
			const string productsBatchKey = "products";
			
			var batch = uow.Session.CreateQueryBatch();

			var templateBatch = _onlineOrderTemplateRepository.GetQueryOverOnlineOrderTemplateDataByTemplateId(templateId);
			var counterpartyBatch = _onlineOrderTemplateRepository.GetQueryOverOnlineOrderTemplateCounterpartyDataByTemplateId(templateId);
			var deliveryPointBatch = _onlineOrderTemplateRepository.GetQueryOverOnlineOrderTemplateDeliveryPointDataByTemplateId(templateId);
			var weekdaysBatch = _onlineOrderTemplateRepository.GetQueryOverOnlineOrderTemplateWeekdaysByTemplateId(templateId);
			var templateProductsBatch = _onlineOrderTemplateRepository.GetQueryOverOnlineOrderTemplateProductsByTemplateId(templateId);

			batch
				.Add<OnlineOrderTemplateInfo>(templateBatchKey, templateBatch)
				.Add<Counterparty>(counterpartyBatchKey, counterpartyBatch)
				.Add<DeliveryPoint>(deliveryPointBatchKey, deliveryPointBatch)
				.Add<string>(weekdaysBatchKey, weekdaysBatch)
				.Add<OnlineOrderTemplateProduct>(productsBatchKey, templateProductsBatch)
				;

			await batch.ExecuteAsync();

			var templateData = (await batch.GetResultAsync<OnlineOrderTemplateInfo>(templateBatchKey))
				.FirstOrDefault();
			var counterparty = (await batch.GetResultAsync<Counterparty>(counterpartyBatchKey))
				.FirstOrDefault();
			var deliveryPoint = (await batch.GetResultAsync<DeliveryPoint>(deliveryPointBatchKey))
				.FirstOrDefault();
			var weekdays = await batch.GetResultAsync<string>(weekdaysBatchKey);
			var items = await batch.GetResultAsync<OnlineOrderTemplateProduct>(productsBatchKey);

			if(templateData is null)
			{
				return null;
			}

			var promoSetsIds = items
				.Where(x => x.PromoSetId.HasValue)
				.ToLookup(x => x.PromoSetId.Value);

			var templateProducts = new List<OrderTemplateProductDto>();
			decimal orderSum = 0;

			var productsWithoutPromoSets = items.Where(x => !x.PromoSetId.HasValue).ToList();

			ProcessItemsWithoutPromoSets(productsWithoutPromoSets, items, deliveryPoint, counterparty, templateProducts, ref orderSum);
			ProcessPromoSets(uow, promoSetsIds, templateProducts, items, deliveryPoint, counterparty, ref orderSum);

			var lastExternalOnlineOrderId = _onlineOrderRepository.GetLastOnlineOrderExternalId(uow, templateData.CounterpartyId);

			var data = OrderTemplateData.Create(
				templateData.Id,
				templateData.IsActive,
				templateData.DeliveryAddress,
				templateData.DeliverySchedule,
				weekdays,
				templateData.RepeatOrder
				);

			var onlineOrderTemplateInfo = OrderTemplateInfoDto.Create(
				data,
				templateData.PaymentType,
				lastExternalOnlineOrderId,
				templateProducts,
				orderSum);

			return onlineOrderTemplateInfo;
		}

		public OrderTemplatesDto GetOnlineOrdersTemplatesList(IUnitOfWork uow, int counterpartyId, int skip, int take)
		{
			var templatesCount = _onlineOrderTemplateRepository.GetOnlineOrdersTemplatesCount(uow, counterpartyId);
			
			var orderTemplatesDictionary = _onlineOrderTemplateRepository
				.GetOnlineOrdersTemplatesDataByCounterpartyId(uow, counterpartyId, skip, take)
				.ToDictionary(x => x.Id);
			
			var weekdays =  _onlineOrderTemplateRepository
				.GetOnlineOrdersTemplatesWeekdaysData(uow, counterpartyId, skip, take)
				.ToLookup(x => x.TemplateId);
			
			var templates = new List<OrderTemplateCardFromListDto>();

			foreach(var keyPairValue in orderTemplatesDictionary)
			{
				var templateWeekdays = new List<string>();
				var currentDayOfWeek = DateTime.Today.DayOfWeek;
				DateTime? nextDeliveryDate = null;
				
				if(weekdays.Contains(keyPairValue.Key))
				{
					templateWeekdays
						.AddRange(weekdays[keyPairValue.Key]
							.Select(x => x.Weekday));

					nextDeliveryDate = CalculateNextDeliveryDate(templateWeekdays, currentDayOfWeek);
				}

				var template = keyPairValue.Value;

				templates.Add(
					OrderTemplateCardFromListDto.Create(
						OrderTemplateData.Create(
							keyPairValue.Key,
							template.IsActive,
							template.DeliveryAddress,
							template.DeliverySchedule,
							templateWeekdays,
							template.RepeatOrder),
						nextDeliveryDate)
					);
			}

			return OrderTemplatesDto.Create(templatesCount, templates);
		}

		private void ProcessItemsWithoutPromoSets(
			IEnumerable<OnlineOrderTemplateProduct> productsWithoutPromoSets,
			IEnumerable<OnlineOrderTemplateProduct> items,
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
				
				var discounts = item.Discounts
					.Select(x =>
						DiscountData.Create(
							x.IsDiscountInMoney ? x.MoneyDiscount : x.PercentDiscount,
							x.IsDiscountInMoney,
							x.DiscountReason.Id))
					.ToArray();

				var onlineOrderItem = OrderTemplateProductDto.Create(
					item.Nomenclature.Id,
					price,
					item.Count,
					item.Id,
					discounts);

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
						var discounts = new[] { DiscountData.Create(promoItem.Discount, promoItem.IsDiscountInMoney) };
						
						var orderTemplateProduct = OrderTemplateProductDto.Create(
							promoItem.Nomenclature.Id,
							_priceCalculator.CalculateItemPrice(
								items,
								deliveryPoint,
								counterparty,
								promoItem,
								false),
							promoItem.Count,
							promoSet.Id,
							discounts);
						
						templateProducts.Add(orderTemplateProduct);
						orderSum += orderTemplateProduct.Sum;
					}
				}
			}
		}
		
		private DateTime? CalculateNextDeliveryDate(
			IEnumerable<string> templateWeekdays,
			DayOfWeek currentDayOfWeek)
		{
			DateTime? nextDeliveryDate = null;
			DayOfWeek? firstTemplateDayOfWeek = null;

			foreach(var templateWeekday in templateWeekdays)
			{
				var weekday = templateWeekday.TryParseAsEnum<WeekDayName>();

				if(weekday != null)
				{
					var dayOfWeek = weekday.Value.ToDayOfWeek();

					if(firstTemplateDayOfWeek is null)
					{
						firstTemplateDayOfWeek = dayOfWeek;
					}

					//TODO 5695: текущий день должен считаться следующей датой?
					if(currentDayOfWeek <= dayOfWeek)
					{
						nextDeliveryDate = DateTime.Today.AddDays(dayOfWeek - currentDayOfWeek);
						break;
					}
				}
			}

			if(nextDeliveryDate is null && firstTemplateDayOfWeek != null)
			{
				nextDeliveryDate = DateTime.Today.AddDays(7 - (int)currentDayOfWeek + (int)firstTemplateDayOfWeek.Value);
			}

			return nextDeliveryDate;
		}
	}
}
