using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Nodes;

namespace Vodovoz.Controllers
{
	public class NomenclatureOnlineParametersController : INomenclatureOnlineParametersController
	{
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IStockRepository _stockRepository;

		public NomenclatureOnlineParametersController(
			INomenclatureRepository nomenclatureRepository,
			IStockRepository stockRepository)
		{
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
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
	}
}
