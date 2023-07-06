using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Stock;

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
		
		public IDictionary<int, NomenclatureOnlineParameters> GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, NomenclatureOnlineParameterType parameterType)
		{
			var parameters =
				_nomenclatureRepository.GetNomenclaturesOnlineParametersForSend(uow, parameterType)
					.ToDictionary(x => x.Nomenclature.Id);

			var nomenclaturesIds =
				parameters.Where(x => x.Value.NomenclatureOnlineAvailability == NomenclatureOnlineAvailability.ShowAndSale)
					.Select(x => x.Key);
			
			var stocksForShowAndSellParams = _stockRepository.NomenclatureInStock(uow, nomenclaturesIds.ToArray());

			foreach(var keyPairValue in stocksForShowAndSellParams.Where(keyPairValue => keyPairValue.Value <= 0))
			{
				parameters[keyPairValue.Key].NomenclatureOnlineAvailability = NomenclatureOnlineAvailability.Show;
			}

			return parameters;
		}
	}
}
