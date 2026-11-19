using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Factories;
using VodovozBusiness.Nodes;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class OnlineOrdersFromTemplatesCreator : IOnlineOrdersFromTemplatesCreator
	{
		private readonly ILogger<OnlineOrdersFromTemplatesCreator> _logger;
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly IGoodsPriceCalculator _goodsPriceCalculator;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IOnlineOrderTemplateRepository _onlineOrderTemplateRepository;
		private readonly IOnlineOrderValidatorCreator _onlineOrderValidatorCreator;
		private readonly ISaleDiscountController _discountController;
		private readonly IOnlineOrderStateHandler _onlineOrderStateHandler;
		private readonly ISaleHandler _saleHandler;
		private readonly IOrderService _orderService;
		private readonly IRouteListService _routeListService;

		public OnlineOrdersFromTemplatesCreator(
			ILogger<OnlineOrdersFromTemplatesCreator> logger,
			IDiscountReasonSettings discountReasonSettings,
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderFactory onlineOrderFactory,
			IOnlineOrderTemplateRepository onlineOrderTemplateRepository,
			IOnlineOrderValidatorCreator onlineOrderValidatorCreator,
			ISaleDiscountController discountController,
			IOnlineOrderStateHandler onlineOrderStateHandler,
			ISaleHandler saleHandler,
			IOrderService orderService,
			IRouteListService routeListService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			_goodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_onlineOrderTemplateRepository = onlineOrderTemplateRepository ?? throw new ArgumentNullException(nameof(onlineOrderTemplateRepository));
			_onlineOrderValidatorCreator = onlineOrderValidatorCreator ?? throw new ArgumentNullException(nameof(onlineOrderValidatorCreator));
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
			_onlineOrderStateHandler = onlineOrderStateHandler ?? throw new ArgumentNullException(nameof(onlineOrderStateHandler));
			_saleHandler = saleHandler ?? throw new ArgumentNullException(nameof(saleHandler));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
		}
		
		public async Task CreateAsync(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начинаем работу по созданию онлайн заказов из шаблонов");
			
			//TODO-5696 создание заказов за два дня. Заменить на параметр
			var deliveryDate = DateTime.Today.AddDays(2);
			var activeTemplateIds =
				await _onlineOrderTemplateRepository.GetActiveOnlineOrdersTemplatesForCreateOrdersAsync(uow, deliveryDate, cancellationToken);

			if(!activeTemplateIds.Any())
			{
				_logger.LogInformation("Нет активных шаблонов на обработку...");
				return;
			}

			var templatesWithItems = await _onlineOrderTemplateRepository.GetOnlineOrdersTemplatesDataAsync(
				uow, activeTemplateIds);

			_logger.LogInformation("Всего шаблонов на обработку: {TemplatesCount}", templatesWithItems.Templates.Count());

			foreach(var template in templatesWithItems.Templates)
			{
				var templateSumAndSaleItemInfo = await ProcessItems(uow, template, templatesWithItems, cancellationToken);

				var onlineOrder = _onlineOrderFactory.Create(
					uow,
					template,
					templateSumAndSaleItemInfo.TemplateSum,
					deliveryDate,
					templateSumAndSaleItemInfo.SaleItemsWithSum,
					templatesWithItems.PromoSetsLookup[template.Id],
					templatesWithItems.RentPackagesLookup[template.Id]
					);

				var validator = _onlineOrderValidatorCreator.Create(onlineOrder);

				if(validator
					.Validate(uow, true)
					.IsFailure)
				{
					_logger.LogInformation(
						"Автозаказ {OnlineOrderId} не прошел проверки, отправляем на ручное...",
						onlineOrder.Id);
					
					_onlineOrderStateHandler.AutoOrderToManualProcessing(onlineOrder);
				}
				else if(onlineOrder.Template.NeedAdvancePayment())
				{
					_logger.LogInformation(
						"Автозаказ {OnlineOrderId} прошел проверки. Это онлайн оплата, на списание средств...",
						onlineOrder.Id);
					
					onlineOrder.OnlineOrderStatus = OnlineOrderStatus.AutoOrderDeductionFunds;
				}
				else
				{
					_logger.LogInformation("Проводим заказ на основе онлайн заказа {OnlineOrderId}", onlineOrder.Id);
					await _orderService.TryCreateOrderFromOnlineOrderAndAcceptAsync(
						uow,
						onlineOrder,
						_routeListService,
						cancellationToken
					);
				}
				
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}
		}

		/// <summary>
		/// Обновление позиций шаблона на момент цен, доставки и скидки за автозаказ
		/// Т.к. шаблон может находиться активным сколь угодно долго, то за это время могут поменяться цены, условия доставки
		/// поэтому эти данные нужно обновлять при создании из него онлайн заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="template">Шаблон автозаказа</param>
		/// <param name="templatesWithItems">Данные по шаблонам и их товарам</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		private async Task<(decimal TemplateSum, IEnumerable<(decimal Sum, OnlineOrderTemplateSaleItem SaleItem)> SaleItemsWithSum)> ProcessItems(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			OnlineOrdersTemplatesData templatesWithItems,
			CancellationToken cancellationToken
			)
		{
			var templateSum = 0m;
			
			var templateSaleItems = templatesWithItems.SaleItemsLookup[template.Id];
			var deliveryPointFixedPrices = templatesWithItems.DeliveryPointFixedPricesLookup[template.DeliveryPointId];
			var counterpartyFixedPrices = templatesWithItems.CounterpartyFixedPricesLookup[template.CounterpartyId];
			var templatePromoSets = templatesWithItems.PromoSetsLookup[template.Id];
			var templateRentPackages = templatesWithItems.RentPackagesLookup[template.Id];
			
			var saleItemsInfo =
				await UpdateSaleItemsAsync(uow, templateSaleItems, deliveryPointFixedPrices, counterpartyFixedPrices, cancellationToken);

			templateSum += saleItemsInfo.Sum(x => x.Sum);
			templateSum += await UpdatePromoItemsAsync(uow, templatePromoSets, cancellationToken);
			templateSum += await UpdateRentPackagesAsync(uow, templateRentPackages, cancellationToken);

			return (templateSum, saleItemsInfo);
		}

		private async Task<IEnumerable<(decimal Sum, OnlineOrderTemplateSaleItem SaleItem)>> UpdateSaleItemsAsync(
			IUnitOfWork uow,
			IEnumerable<OnlineOrderTemplateSaleItem> saleItems,
			IEnumerable<NomenclatureFixedPrice> deliveryPointFixedPrices,
			IEnumerable<NomenclatureFixedPrice> counterpartyFixedPrices,
			CancellationToken cancellationToken
			)
		{
			var list = new List<(decimal Sum, OnlineOrderTemplateSaleItem SaleItem)>();
			
			_saleHandler.UpdateDeliveryCost(uow);
			
			foreach(var saleItem in saleItems)
			{
				var sum = 0m;
				var discount = 0m;
				
				var price = _goodsPriceCalculator.CalculateItemPrice(
					saleItems,
					deliveryPointFixedPrices,
					counterpartyFixedPrices,
					saleItem,
					false
					);

				/*if(saleItem.Price == price.Price)
				{
					discount = _discountController
						.CalculateTotalDiscountDetails(saleItem)
						.TotalDiscount;
					
					sum = saleItem.CurrentRawPrice - discount;
					list.Add((sum, saleItem));
					continue;
				}*/
				
				AddAutoOrderDiscountIfNeeded(uow, saleItem);

				saleItem.Price = price.Price;
				discount = _discountController
					.CalculateTotalDiscountDetails(saleItem)
					.TotalDiscount;
					
				sum = saleItem.CurrentRawPrice - discount;
				list.Add((sum, saleItem));

				await uow.SaveAsync(saleItem, cancellationToken: cancellationToken);
			}

			return list;
		}

		private async Task<decimal> UpdatePromoItemsAsync(
			IUnitOfWork uow,
			IEnumerable<OnlineOrderTemplatePromoSet> templatePromoSets,
			CancellationToken cancellationToken
		)
		{
			var sum = 0m; 
			
			foreach(var templatePromoSet in templatePromoSets)
			{
				var promoSetCurrentPrice = templatePromoSet.PromoSet.Sum();
				
				AddAutoOrderDiscountIfNeeded(uow, templatePromoSet);
				
				templatePromoSet.Price = promoSetCurrentPrice;
				var discountData = _discountController.CalculateTotalDiscountDetails(templatePromoSet);
				sum += templatePromoSet.CurrentRawPrice - discountData.TotalDiscount;
				
				await uow.SaveAsync(templatePromoSet, cancellationToken: cancellationToken);
			}
			
			return sum;
		}
		
		private async Task<decimal> UpdateRentPackagesAsync(
			IUnitOfWork uow,
			IEnumerable<OnlineOrderTemplateFreeRentPackage> templateRentPackages,
			CancellationToken cancellationToken
		)
		{
			var sum = 0m;
			
			foreach(var templateRentPackage in templateRentPackages)
			{
				var rentPackageCurrentPrice = templateRentPackage.FreeRentPackage.Deposit;

				templateRentPackage.Price = rentPackageCurrentPrice;
				sum += templateRentPackage.CurrentRawPrice;
				
				await uow.SaveAsync(templateRentPackage, cancellationToken: cancellationToken);
			}
			
			return sum;
		}

		private void AddAutoOrderDiscountIfNeeded(IUnitOfWork uow, ICalculatingTotalMoneyDiscount saleItem)
		{
			var autoOrderDiscount = uow.GetById<AutoOrderDiscount>(_discountReasonSettings.AutoOrderDiscountReasonId);

			if(autoOrderDiscount is null)
			{
				throw new InvalidOperationException(
					"Не найдена скидка за автозаказ!. Проверьте параметр DiscountReason.AutoOrderDiscountReasonId в базе или само основание");
			}

			var applicablePromotion = ApplicablePromotion.Create(saleItem);
			
			//TODO-5696 проверить расчет скидки с персональной скидкой, если будет превышение лимита 100% или рублевого эквивалента
			//TODO-5696 научить добавлять скидку в промонаборы
			//var applyDiscountItem = saleItem.ToApplyDiscountReasonItem();
			//_discountController.AddDiscountFromDiscountReason(autoOrderDiscount, applyDiscountItem);
		}
	}
}
