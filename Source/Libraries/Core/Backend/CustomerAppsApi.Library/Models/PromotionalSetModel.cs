using System;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Goods;
using CustomerAppsApi.Library.Factories;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Models
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
