using System;
using CustomerAppsApi.Library.V2.Converters;
using CustomerAppsApi.Library.V2.Dto.Goods;
using CustomerAppsApi.Library.V2.Factories;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Models
{
	public class PromotionalSetModel : IPromotionalSetModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGoodsOnlineParametersController _goodsOnlineParametersController;
		private readonly ISourceConverter _sourceConverter;
		private readonly IPromotionalSetFactory _promotionalSetFactory;

		public PromotionalSetModel(
			IUnitOfWork unitOfWork,
			IGoodsOnlineParametersController goodsOnlineParametersController,
			ISourceConverter sourceConverter,
			IPromotionalSetFactory promotionalSetFactory)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_goodsOnlineParametersController =
				goodsOnlineParametersController ?? throw new ArgumentNullException(nameof(goodsOnlineParametersController));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_promotionalSetFactory = promotionalSetFactory ?? throw new ArgumentNullException(nameof(promotionalSetFactory));
		}

		public PromotionalSetsDto GetPromotionalSets(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var parametersData = _goodsOnlineParametersController.GetPromotionalSetsOnlineParametersForSend(_unitOfWork, parameterType);

			return _promotionalSetFactory.CreatePromotionalSetsDto(parametersData);
		}
	}
}
