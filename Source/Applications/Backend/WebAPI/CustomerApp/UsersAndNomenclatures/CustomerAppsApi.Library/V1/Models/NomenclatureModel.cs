using System;
using CustomerAppsApi.Library.V1.Converters;
using CustomerAppsApi.Library.V1.Dto.Goods;
using CustomerAppsApi.Library.V1.Factories;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V1.Models
{
	public class NomenclatureModel : INomenclatureModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGoodsOnlineParametersController _goodsOnlineParametersController;
		private readonly ISourceConverter _sourceConverter;
		private readonly INomenclatureFactory _nomenclatureFactory;
		private readonly INomenclatureOnlineCharacteristicsConverter _nomenclatureOnlineCharacteristicsConverter;

		public NomenclatureModel(
			IUnitOfWork unitOfWork,
			IGoodsOnlineParametersController goodsOnlineParametersController,
			ISourceConverter sourceConverter,
			INomenclatureFactory nomenclatureFactory,
			INomenclatureOnlineCharacteristicsConverter nomenclatureOnlineCharacteristicsConverter)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_goodsOnlineParametersController =
				goodsOnlineParametersController ?? throw new ArgumentNullException(nameof(goodsOnlineParametersController));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_nomenclatureFactory = nomenclatureFactory ?? throw new ArgumentNullException(nameof(nomenclatureFactory));
			_nomenclatureOnlineCharacteristicsConverter =
				nomenclatureOnlineCharacteristicsConverter ?? throw new ArgumentNullException(nameof(nomenclatureOnlineCharacteristicsConverter));
		}

		public NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var parametersData = _goodsOnlineParametersController.GetNomenclaturesOnlineParametersForSend(_unitOfWork, parameterType);

			return _nomenclatureFactory.CreateNomenclaturesPricesAndStockDto(parametersData);
		}
		
		public NomenclaturesDto GetNomenclatures(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var nomenclatureCharacteristics =
				_goodsOnlineParametersController.GetNomenclaturesForSend(_unitOfWork, parameterType);

			return _nomenclatureFactory.CreateNomenclaturesDto(_nomenclatureOnlineCharacteristicsConverter, nomenclatureCharacteristics);
		}
	}
}
