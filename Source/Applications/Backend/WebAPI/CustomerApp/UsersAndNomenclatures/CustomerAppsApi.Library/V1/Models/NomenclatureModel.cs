using System;
using System.Linq;
using CustomerAppsApi.Library.V1.Converters;
using CustomerAppsApi.Library.V1.Dto.Goods;
using CustomerAppsApi.Library.V1.Factories;
using CustomerAppsApi.Library.V1.Repositories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Settings.Common;

namespace CustomerAppsApi.Library.V1.Models
{
	public class NomenclatureModel : INomenclatureModel
	{
		private readonly ILogger<NomenclatureModel> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGeneralSettings _generalSettings;
		private readonly IStockRepository _stockRepository;
		private readonly ISourceConverter _sourceConverter;
		private readonly INomenclatureFactory _nomenclatureFactory;
		private readonly ISaleItemRepository _saleItemRepository;

		public NomenclatureModel(
			ILogger<NomenclatureModel> logger,
			IUnitOfWork unitOfWork,
			IGeneralSettings generalSettings,
			IStockRepository stockRepository,
			ISourceConverter sourceConverter,
			INomenclatureFactory nomenclatureFactory,
			ISaleItemRepository saleItemRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_nomenclatureFactory = nomenclatureFactory ?? throw new ArgumentNullException(nameof(nomenclatureFactory));
			_saleItemRepository = saleItemRepository ?? throw new ArgumentNullException(nameof(saleItemRepository));
		}

		public NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var parameters =
				_saleItemRepository.GetActiveNomenclaturesOnlineParametersForSend(_unitOfWork, parameterType)
					.ToDictionary(x => x.NomenclatureId);
			
			var prices =
				_saleItemRepository.GetNomenclaturesOnlinePricesByOnlineParameters(_unitOfWork, parameters.Select(x => x.Value.Id))
					.ToLookup(x => x.NomenclatureOnlineParametersId);

			var nomenclaturesIds =
				parameters.Where(x => x.Value.AvailableForSale == GoodsOnlineAvailability.ShowAndSale)
					.Select(x => x.Key);
			
			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;
			_logger.LogInformation("Считаем баланс по номенклатурам");
			var stocksForShowAndSellParams = _stockRepository.NomenclatureInStock(_unitOfWork, nomenclaturesIds.ToArray(), warehouses);
			_logger.LogInformation("Посчитали баланс по номенклатурам");

			//Если по выбранным складам не было движений номенклатуры, то она будет отсутствовать в итоговой выборке.
			//Поэтому просто берем баланс по Id номенклатуры и если ее там нет, вернется дефолтное значение 0
			foreach(var parameter in parameters)
			{
				stocksForShowAndSellParams.TryGetValue(parameter.Key, out var balance);

				if(balance <= 0 && !parameter.Value.IsService)
				{
					parameter.Value.AvailableForSale = GoodsOnlineAvailability.Show;
				}
			}

			return _nomenclatureFactory.CreateNomenclaturesPricesAndStockDto(parameters, prices);
		}

		public NomenclaturesDto GetNomenclatures(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			return _nomenclatureFactory.CreateNomenclaturesDto(_saleItemRepository.GetNomenclaturesForSend(_unitOfWork, parameterType));
		}
	}
}
