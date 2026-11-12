using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Errors.Orders;
using Vodovoz.Settings.Common;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OnlineOrderFromTemplateValidator
	{
		private readonly ILogger<OnlineOrderFromTemplateValidator> _logger;
		private readonly IGeneralSettings _generalSettings;
		private readonly IStockRepository _stockRepository;

		public OnlineOrderFromTemplateValidator(
			ILogger<OnlineOrderFromTemplateValidator> logger,
			IGeneralSettings generalSettings,
			IStockRepository stockRepository,
			INomenclatureRepository nomenclatureRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
		}
		
		public Result Validate(IUnitOfWork uow, OnlineOrderV2 onlineOrder)
		{
			var errors = new List<Error>();
			
			if(onlineOrder.Template is null)
			{
				return Result.Success();
			}
			
			ValidateItems(uow, onlineOrder, errors);
			
			return Result.Success();
		}

		private void ValidateItems(
			IUnitOfWork uow,
			OnlineOrderV2 onlineOrder,
			ICollection<Error> errors
			)
		{
			var nomenclatureIds = onlineOrder.SaleItems
				.Where(x => x.Nomenclature != null)
				.Select(x => x.Nomenclature.Id)
				.ToArray();
			
			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;
			var stockData = _stockRepository.NomenclatureInStock(uow, nomenclatureIds, warehouses);
			
			foreach(var saleItem in onlineOrder.SaleItems)
			{
				var nomenclature = saleItem.Nomenclature;
				
				if(nomenclature.Category != NomenclatureCategory.master
					&& nomenclature.Category != NomenclatureCategory.service
					&& (!stockData.TryGetValue(saleItem.Nomenclature.Id, out var stock)
						|| stock == 0m))
				{
					errors.Add(OnlineOrderErrors.NomenclatureIsOutOfStock(nomenclature.Id, nomenclature.Name));
				}

				//TODO-5696 проверка на доступность к продаже номенклатуры
				if(true)
				{
					
				}
			}
		}
	}
}
