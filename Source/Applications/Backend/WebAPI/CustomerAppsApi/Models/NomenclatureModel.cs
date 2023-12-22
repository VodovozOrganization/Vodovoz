using System;
using CustomerAppsApi.Converters;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Dto;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;

namespace CustomerAppsApi.Models
{
	public class NomenclatureModel : INomenclatureModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGoodsOnlineParametersController _goodsOnlineParametersController;
		private readonly ISourceConverter _sourceConverter;
		private readonly INomenclatureFactory _nomenclatureFactory;

		public NomenclatureModel(
			IUnitOfWork unitOfWork,
			IGoodsOnlineParametersController goodsOnlineParametersController,
			ISourceConverter sourceConverter,
			INomenclatureFactory nomenclatureFactory)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_goodsOnlineParametersController =
				goodsOnlineParametersController ?? throw new ArgumentNullException(nameof(goodsOnlineParametersController));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_nomenclatureFactory = nomenclatureFactory ?? throw new ArgumentNullException(nameof(nomenclatureFactory));
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
				_nomenclatureOnlineParametersController.GetNomenclaturesForSend(_unitOfWork, parameterType);

			return _nomenclatureFactory.CreateNomenclaturesDto(nomenclatureCharacteristics);
		}
	}
}
