using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Goods.Recomendations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Application.Goods
{
	internal sealed class RecomendationService : IRecomendationService
	{
		private readonly IGenericRepository<Recomendation> _recomendationsRepository;
		private readonly IRecomendationSettings _recomendationSettings;

		public RecomendationService(
			IGenericRepository<Recomendation> recomendationsRepository,
			IRecomendationSettings recomendationSettings)
		{
			_recomendationsRepository = recomendationsRepository
				?? throw new ArgumentNullException(nameof(recomendationsRepository));
			_recomendationSettings = recomendationSettings
				?? throw new ArgumentNullException(nameof(recomendationSettings));
		}

		public IEnumerable<RecomendationItem> GetRecomendationItemsForIpz(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(unitOfWork, personType, roomType, excludeNomenclatures, _recomendationSettings.IpzCount);

		public IEnumerable<RecomendationItem> GetRecomendationItemsForOperator(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(unitOfWork, personType, roomType, excludeNomenclatures, _recomendationSettings.OperatorCount);

		public IEnumerable<RecomendationItem> GetRecomendationItemsForRobot(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(unitOfWork, personType, roomType, excludeNomenclatures, _recomendationSettings.RobotCount);

		private IEnumerable<RecomendationItem> GetRecomendationItems(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures, int limit)
		{
			var baseRecomendation = _recomendationsRepository
				.GetFirstOrDefault(
					unitOfWork,
					x => !x.IsArchive
						&& x.PersonType == null
						&& x.RoomType == null);

			var concreteRecomendation = _recomendationsRepository
				.Get(
					unitOfWork,
					x => !x.IsArchive
						&& (x.PersonType != null || x.RoomType != null)
						&& (x.PersonType == null || x.PersonType == personType)
						&& (x.RoomType == null || x.RoomType == roomType))
				.OrderBy(x => x.RoomType == null | x.PersonType == null)
				.FirstOrDefault();

			var recomendationItems = new List<RecomendationItem>();

			var baseRecomendationItem = baseRecomendation.Items
				.OrderBy(x => x.Priority)
				.FirstOrDefault(x => !excludeNomenclatures.Contains(x.NomenclatureId));

			if(baseRecomendationItem != null)
			{
				recomendationItems.Add(baseRecomendationItem);
			}

			if(concreteRecomendation is null)
			{
				return recomendationItems;
			}

			foreach(var recomendationItem in concreteRecomendation.Items.OrderBy(x => x.Priority))
			{
				if(recomendationItems.Count >= limit)
				{
					break;
				}

				if(recomendationItems.Any(x => x.NomenclatureId == recomendationItem.NomenclatureId)
					|| excludeNomenclatures.Contains(recomendationItem.NomenclatureId))
				{
					continue;
				}

				recomendationItems.Add(recomendationItem);
			}

			return recomendationItems;
		}
	}
}
