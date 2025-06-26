using FluentNHibernate.Conventions;
using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

		public IEnumerable<RecomendationItem> GetRecomendationItemsForIpz(
			IUnitOfWork unitOfWork,
			Source source,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(unitOfWork, personType, roomType, excludeNomenclatures, _recomendationSettings.IpzCount);

		public IEnumerable<RecomendationItem> GetRecomendationItemsForOperator(
			IUnitOfWork unitOfWork,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(unitOfWork, personType, roomType, excludeNomenclatures, _recomendationSettings.OperatorCount);

		public IEnumerable<RecomendationItem> GetRecomendationItemsForRobot(
			IUnitOfWork unitOfWork,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(unitOfWork, personType, roomType, excludeNomenclatures, _recomendationSettings.RobotCount);

		private IEnumerable<RecomendationItem> GetRecomendationItems(
			IUnitOfWork unitOfWork,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures,
			int limit)
		{
			if(limit <= 0)
			{
				return Enumerable.Empty<RecomendationItem>();
			}

			var baseRecomendation = _recomendationsRepository
				.GetFirstOrDefault(
					unitOfWork,
					x => !x.IsArchive
						&& x.PersonType == null
						&& x.RoomType == null);

			var specifiedRecomendation = _recomendationsRepository
				.Get(
					unitOfWork,
					x => !x.IsArchive
						&& (x.PersonType != null || x.RoomType != null)
						&& (x.PersonType == null || x.PersonType == personType)
						&& (x.RoomType == null || x.RoomType == roomType))
				.OrderBy(x => x.RoomType != roomType)
				.ThenBy(x => x.PersonType != personType)
				.FirstOrDefault();

			var baseRecomendationItems = baseRecomendation?.Items
				.OrderBy(x => x.Priority)
				.Where(x => !excludeNomenclatures.Contains(x.NomenclatureId))
				.ToArray() ?? Enumerable.Empty<RecomendationItem>();

			var specifiedRecomendationItems = specifiedRecomendation?.Items
				.OrderBy(x => x.Priority)
				.Where(x => !excludeNomenclatures.Contains(x.NomenclatureId))
				.ToArray() ?? Enumerable.Empty<RecomendationItem>();

			var baseCount = baseRecomendationItems.Count();

			var specifiedCount = specifiedRecomendationItems.Count();

			var baseCountToTake = Math.Max(1, limit - specifiedCount);
			var specifiedCountToTake = Math.Max(0, limit - baseCount > 0 ? 1 : 0);

			var recomendationItems = new List<RecomendationItem>();

			recomendationItems.AddRange(baseRecomendationItems.Take(baseCountToTake));

			recomendationItems.AddRange(specifiedRecomendationItems
				.Where(x => !recomendationItems
					.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(specifiedCountToTake));

			return recomendationItems;
		}
	}
}
