using System;
using System.Linq;
using CustomerAppsApi.Library.V1.Converters;
using CustomerAppsApi.Library.V1.Dto.Goods;
using CustomerAppsApi.Library.V1.Factories;
using CustomerAppsApi.Library.V1.Repositories;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Settings.Common;

namespace CustomerAppsApi.Library.V1.Models
{
	public class PromotionalSetModel : IPromotionalSetModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGeneralSettings _generalSettings;
		private readonly ICustomerAppPromotionalSetRepository _promotionalSetRepository;
		private readonly ISourceConverter _sourceConverter;
		private readonly IPromotionalSetFactory _promotionalSetFactory;

		public PromotionalSetModel(
			IUnitOfWork unitOfWork,
			IGeneralSettings generalSettings,
			ICustomerAppPromotionalSetRepository promotionalSetRepository,
			ISourceConverter sourceConverter,
			IPromotionalSetFactory promotionalSetFactory)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_promotionalSetRepository =
				promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_promotionalSetFactory = promotionalSetFactory ?? throw new ArgumentNullException(nameof(promotionalSetFactory));
		}

		public PromotionalSetsDto GetPromotionalSets(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var parameters =
				_promotionalSetRepository.GetActivePromotionalSetsOnlineParametersForSend(_unitOfWork, parameterType)
					.ToDictionary(x => x.PromotionalSetId);

			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;
			var promotionalSetItems =
				_promotionalSetRepository.GetPromotionalSetsItemsWithBalanceForSend(_unitOfWork, parameterType, warehouses)
					.ToLookup(x => x.PromotionalSetId);

			var itemsWithZeroBalance =
				promotionalSetItems.SelectMany(keyPairValue =>
					keyPairValue.Where(x => x.Stock <= 0));

			foreach(var item in itemsWithZeroBalance)
			{
				parameters[item.PromotionalSetId].AvailableForSale = GoodsOnlineAvailability.Show;
			}

			return _promotionalSetFactory.CreatePromotionalSetsDto(parameters, promotionalSetItems);
		}
	}
}
