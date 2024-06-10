﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Nodes;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Controllers
{
	public class GoodsOnlineParametersController : IGoodsOnlineParametersController
	{
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IStockRepository _stockRepository;
		private readonly IGeneralSettings _generalSettings;

		public GoodsOnlineParametersController(
			INomenclatureRepository nomenclatureRepository,
			IPromotionalSetRepository promotionalSetRepository,
			IStockRepository stockRepository,
			IGeneralSettings generalSettings)
		{
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
		}
		
		public NomenclatureOnlineParametersData GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var parameters =
				_nomenclatureRepository.GetActiveNomenclaturesOnlineParametersForSend(uow, parameterType)
					.ToDictionary(x => x.NomenclatureId);
			
			var prices =
				_nomenclatureRepository.GetNomenclaturesOnlinePricesByOnlineParameters(uow, parameters.Select(x => x.Value.Id))
					.ToLookup(x => x.NomenclatureOnlineParametersId);

			var nomenclaturesIds =
				parameters.Where(x => x.Value.AvailableForSale == GoodsOnlineAvailability.ShowAndSale)
					.Select(x => x.Key);
			
			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;
			var stocksForShowAndSellParams = _stockRepository.NomenclatureInStock(uow, nomenclaturesIds.ToArray(), warehouses);

			foreach(var keyPairValue in stocksForShowAndSellParams.Where(keyPairValue => keyPairValue.Value <= 0))
			{
				parameters[keyPairValue.Key].AvailableForSale = GoodsOnlineAvailability.Show;
			}

			return new NomenclatureOnlineParametersData(parameters, prices);
		}

		public IList<OnlineNomenclatureNode> GetNomenclaturesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var nomenclatures = _nomenclatureRepository.GetNomenclaturesForSend(uow, parameterType);

			return nomenclatures;
		}

		public PromotionalSetOnlineParametersData GetPromotionalSetsOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var parameters =
				_promotionalSetRepository.GetActivePromotionalSetsOnlineParametersForSend(uow, parameterType)
					.ToDictionary(x => x.PromotionalSetId);

			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;
			var promotionalSetItems =
				_promotionalSetRepository.GetPromotionalSetsItemsWithBalanceForSend(uow, parameterType, warehouses)
					.ToLookup(x => x.PromotionalSetId);

			var itemsWithZeroBalance =
				promotionalSetItems.SelectMany(keyPairValue =>
					keyPairValue.Where(x => x.Stock <= 0));

			foreach(var item in itemsWithZeroBalance)
			{
				parameters[item.PromotionalSetId].AvailableForSale = GoodsOnlineAvailability.Show;
			}
			
			return new PromotionalSetOnlineParametersData(parameters, promotionalSetItems);
		}
	}
}
