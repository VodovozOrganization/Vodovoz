using System;
using CustomerAppsApi.Library.V2.Converters;
using CustomerAppsApi.Library.V2.Dto.Goods;
using CustomerAppsApi.Library.V2.Factories;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Models
{
	public class NomenclatureModel : INomenclatureModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGoodsOnlineParametersController _goodsOnlineParametersController;
		private readonly ISourceConverter _sourceConverter;
		private readonly ISaleItemFactory _saleItemFactory;
		private readonly INomenclatureOnlineCharacteristicsConverter _nomenclatureOnlineCharacteristicsConverter;

		public NomenclatureModel(
			IUnitOfWork unitOfWork,
			IGoodsOnlineParametersController goodsOnlineParametersController,
			ISourceConverter sourceConverter,
			ISaleItemFactory saleItemFactory,
			INomenclatureOnlineCharacteristicsConverter nomenclatureOnlineCharacteristicsConverter)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_goodsOnlineParametersController =
				goodsOnlineParametersController ?? throw new ArgumentNullException(nameof(goodsOnlineParametersController));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_saleItemFactory = saleItemFactory ?? throw new ArgumentNullException(nameof(saleItemFactory));
			_nomenclatureOnlineCharacteristicsConverter =
				nomenclatureOnlineCharacteristicsConverter ?? throw new ArgumentNullException(nameof(nomenclatureOnlineCharacteristicsConverter));
		}

		public NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var parametersData = _goodsOnlineParametersController.GetNomenclaturesOnlineParametersForSend(_unitOfWork, parameterType);

			return _saleItemFactory.CreateNomenclaturesPricesAndStockDto(parametersData);
		}
		
		public SaleItemsDto GetNomenclatures(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var nomenclatureCharacteristics =
				_goodsOnlineParametersController.GetNomenclaturesForSend(_unitOfWork, parameterType);

			return _saleItemFactory.CreateSaleItemsDto(nomenclatureCharacteristics);
		}
	}
}
