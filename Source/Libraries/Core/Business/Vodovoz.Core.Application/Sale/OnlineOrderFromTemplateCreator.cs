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
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Factories;
using VodovozBusiness.Nodes;

namespace Vodovoz.Core.Application.Sale
{
	public class OnlineOrderFromTemplateCreator
	{
		private readonly ILogger<OnlineOrderFromTemplateCreator> _logger;
		private readonly IGoodsPriceCalculator _goodsPriceCalculator;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IOnlineOrderTemplateRepository _onlineOrderTemplateRepository;
		private readonly IOnlineOrderValidatorCreator _onlineOrderValidatorCreator;
		private readonly IDiscountController _discountController;

		public OnlineOrderFromTemplateCreator(
			ILogger<OnlineOrderFromTemplateCreator> logger,
			IGoodsPriceCalculator goodsPriceCalculator,
			IOnlineOrderFactory onlineOrderFactory,
			IOnlineOrderTemplateRepository onlineOrderTemplateRepository,
			IOnlineOrderValidatorCreator onlineOrderValidatorCreator,
			IDiscountController discountController
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_goodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_onlineOrderTemplateRepository = onlineOrderTemplateRepository ?? throw new ArgumentNullException(nameof(onlineOrderTemplateRepository));
			_onlineOrderValidatorCreator = onlineOrderValidatorCreator ?? throw new ArgumentNullException(nameof(onlineOrderValidatorCreator));
			_discountController = discountController ?? throw new ArgumentNullException(nameof(discountController));
		}
		
		public async Task Create(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начинаем работу по созданию онлайн заказов из шаблонов");
			
			var activeTemplates =
				_onlineOrderTemplateRepository.GetActiveOnlineOrdersTemplatesForCreateOrders(uow, DateTime.Now);

			var templatesWithItems = await _onlineOrderTemplateRepository.GetOnlineOrdersTemplatesDataAsync(
				uow, activeTemplates.Select(x => x.Id).ToArray());

			_logger.LogInformation("Всего шаблонов на обработку: {TemplatesCount}", templatesWithItems.Templates.Count());

			foreach(var template in templatesWithItems.Templates)
			{
				//TODO-5696 обсудить вопрос с назначением даты с Костей, в том числе переносы
				//Подумать над логикой присвоения даты доставки. Может лучше убрать все в одно место?
				//template.DeliveryDate = DateTime.Today.AddDays(1);
				var deliveryDate = DateTime.Today.AddDays(2);
				
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
					onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
				}
				
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}
		}

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
			templateSum += await UpdatePromItemsAsync(uow, templatePromoSets, cancellationToken);
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

				if(saleItem.Price == price.Price)
				{
					discount = _discountController
						.CalculateTotalDiscountDetails(saleItem)
						.TotalDiscount;
					
					sum = saleItem.CurrentRawPrice - discount;
					list.Add((sum, saleItem));
					continue;
				}

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
		
		private async Task<decimal> UpdatePromItemsAsync(
			IUnitOfWork uow,
			IEnumerable<OnlineOrderTemplatePromoSet> templatePromoSets,
			CancellationToken cancellationToken
		)
		{
			var sum = 0m; 
			
			foreach(var templatePromoSet in templatePromoSets)
			{
				var promoSetCurrentPrice = templatePromoSet.PromoSet.Sum();
				
				if(templatePromoSet.Price == promoSetCurrentPrice)
				{
					continue;
				}
				
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

				if(templateRentPackage.Price == rentPackageCurrentPrice)
				{
					continue;
				}
				
				templateRentPackage.Price = rentPackageCurrentPrice;
				sum += templateRentPackage.CurrentRawPrice;
				
				await uow.SaveAsync(templateRentPackage, cancellationToken: cancellationToken);
			}
			
			return sum;
		}
	}
}
