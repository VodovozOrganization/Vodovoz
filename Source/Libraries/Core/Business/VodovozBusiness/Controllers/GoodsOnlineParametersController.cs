using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	public class GoodsOnlineParametersController : IGoodsOnlineParametersController
	{
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IStockRepository _stockRepository;

		public GoodsOnlineParametersController(
			INomenclatureRepository nomenclatureRepository,
			IPromotionalSetRepository promotionalSetRepository,
			IStockRepository stockRepository)
		{
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
		}
		
		public NomenclatureOnlineParametersData GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var parameters =
				_nomenclatureRepository.GetNomenclaturesOnlineParametersForSend(uow, parameterType)
					.ToDictionary(x => x.NomenclatureId);
			
			var prices =
				_nomenclatureRepository.GetNomenclaturesOnlinePricesByOnlineParameters(uow, parameters.Select(x => x.Value.Id))
					.ToLookup(x => x.NomenclatureOnlineParametersId);

			var nomenclaturesIds =
				parameters.Where(x => x.Value.AvailableForSale == GoodsOnlineAvailability.ShowAndSale)
					.Select(x => x.Key);
			
			var stocksForShowAndSellParams = _stockRepository.NomenclatureInStock(uow, nomenclaturesIds.ToArray());

			foreach(var keyPairValue in stocksForShowAndSellParams.Where(keyPairValue => keyPairValue.Value <= 0))
			{
				parameters[keyPairValue.Key].AvailableForSale = GoodsOnlineAvailability.Show;
			}

			return new NomenclatureOnlineParametersData(parameters, prices);
		}
		
		public PromotionalSetOnlineParametersData GetPromotionalSetsOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var parameters =
				_promotionalSetRepository.GetPromotionalSetsOnlineParametersForSend(uow, parameterType)
					.ToDictionary(x => x.PromotionalSetId);
			
			var promotionalSetItems =
				_promotionalSetRepository.GetPromotionalSetsItemsWithBalanceForSend(uow, parameterType)
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
