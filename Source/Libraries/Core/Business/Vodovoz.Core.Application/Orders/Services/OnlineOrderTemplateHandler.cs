using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Infrastructure;
using CustomerOrders.Contracts.V5.Orders.Discounts;
using CustomerOrders.Contracts.V5.Orders.Templates;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Extensions;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderTemplateHandler
	{
		private readonly IGoodsPriceCalculator _priceCalculator;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOnlineOrderTemplateRepository _onlineOrderTemplateRepository;

		public OnlineOrderTemplateHandler(
			IGoodsPriceCalculator priceCalculator,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderTemplateRepository onlineOrderTemplateRepository)
		{
			_priceCalculator = priceCalculator ?? throw new ArgumentNullException(nameof(priceCalculator));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_onlineOrderTemplateRepository = onlineOrderTemplateRepository ?? throw new ArgumentNullException(nameof(onlineOrderTemplateRepository));
		}
		
		public async Task<OrderTemplateInfoDto> GetFreshOnlineOrderTemplateDataAsync(IUnitOfWork uow, int templateId)
		{
			var data = await _onlineOrderTemplateRepository.GetAggregateOnlineOrderTemplateInfoAsync(uow, templateId);
			var templateData = data.Template;
			var counterparty = data.Counterparty;
			var deliveryPoint = data.DeliveryPoint;
			var weekdays = data.Weekdays;
			var products = data.Products;

			if(templateData is null)
			{
				return null;
			}

			var promoSetsIds = products
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSet.Id);

			var templateProducts = new List<OrderTemplateProductDto>();
			decimal orderSum = 0;

			var productsWithoutPromoSets = products.Where(x => x.PromoSet is null).ToList();

			ProcessItemsWithoutPromoSets(productsWithoutPromoSets, products, deliveryPoint, counterparty, templateProducts, ref orderSum);
			ProcessPromoSets(uow, promoSetsIds, templateProducts, products, deliveryPoint, counterparty, ref orderSum);

			var lastOnlineOrderIdFromTemplate = _onlineOrderRepository.GetLastOnlineOrderIdFromTemplate(uow, templateData.CounterpartyId);

			var orderTemplateData = OrderTemplateData.Create(
				templateData.Id,
				templateData.IsActive,
				templateData.DeliveryAddress,
				templateData.DeliverySchedule,
				weekdays,
				templateData.RepeatOrder
				);

			var onlineOrderTemplateInfo = OrderTemplateInfoDto.Create(
				orderTemplateData,
				templateData.PaymentType,
				lastOnlineOrderIdFromTemplate,
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
				
				var template = keyPairValue.Value;
				
				if(weekdays.Contains(keyPairValue.Key))
				{
					templateWeekdays
						.AddRange(weekdays[keyPairValue.Key]
							.Select(x => x.Weekday));

					nextDeliveryDate = CalculateNextDeliveryDate(templateWeekdays, template.RepeatOrder, currentDayOfWeek);
				}

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
				var price =  _priceCalculator.CalculatePrice(
					items,
					counterparty,
					deliveryPoint,
					item.Nomenclature,
					false,
					false);
				
				var discounts = item.Discounts
					.Select(x =>
						DiscountDto.Create(
							x.IsDiscountInMoney,
							x.IsDiscountInMoney ? x.MoneyDiscount : x.PercentDiscount,
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
						var discounts = new[] { DiscountDto.Create(promoItem.Discount, promoItem.IsDiscountInMoney) };
						
						var orderTemplateProduct = OrderTemplateProductDto.Create(
							promoItem.Nomenclature.Id,
							_priceCalculator.CalculatePrice(
								items,
								counterparty,
								deliveryPoint,
								promoItem.Nomenclature,
								true,
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
			string repeatOrder,
			DayOfWeek currentDayOfWeek)
		{
			DateTime? nextDeliveryDate = null;
			DayOfWeek? firstTemplateDayOfWeek = null;
			var deliveryFrequency = repeatOrder.TryParseAsEnum<OnlineOrderDeliveryFrequency>();

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
				var days = 0;
				
				switch(deliveryFrequency)
				{
					case OnlineOrderDeliveryFrequency.OnePerWeek:
						days = 7;
						break;
					case OnlineOrderDeliveryFrequency.OneEveryTwoWeeks:
						days = 14;
						break;
					case OnlineOrderDeliveryFrequency.OneEveryThreeWeeks:
						days = 21;
						break;
					case OnlineOrderDeliveryFrequency.OneEveryFourWeeks:
						days = 28;
						break;
					default:
						throw new InvalidOperationException($"Неизвестный интервал периодичности доставки автозаказа {deliveryFrequency}");
				}
				
				nextDeliveryDate = DateTime.Today.AddDays(days - (int)currentDayOfWeek + (int)firstTemplateDayOfWeek.Value);
			}

			return nextDeliveryDate;
		}
	}
}
