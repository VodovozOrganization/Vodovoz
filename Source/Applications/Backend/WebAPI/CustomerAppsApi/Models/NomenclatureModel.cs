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
		private readonly IUnitOfWork _uow;
		private readonly INomenclatureOnlineParametersController _nomenclatureOnlineParametersController;
		private readonly ISourceConverter _sourceConverter;
		private readonly INomenclatureFactory _nomenclatureFactory;

		public NomenclatureModel(
			IUnitOfWork uow,
			INomenclatureOnlineParametersController nomenclatureOnlineParametersController,
			ISourceConverter sourceConverter,
			INomenclatureFactory nomenclatureFactory)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_nomenclatureOnlineParametersController =
				nomenclatureOnlineParametersController ?? throw new ArgumentNullException(nameof(nomenclatureOnlineParametersController));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_nomenclatureFactory = nomenclatureFactory ?? throw new ArgumentNullException(nameof(nomenclatureFactory));
		}

		public NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var parameters = _nomenclatureOnlineParametersController.GetNomenclaturesOnlineParametersForSend(_uow, parameterType);

			return _nomenclatureFactory.CreateNomenclaturesPricesAndStockDto(parameters);
		}
	}
}
