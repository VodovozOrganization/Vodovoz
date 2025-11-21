using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Goods.Recomendations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Application.Goods
{
	internal sealed partial class RecomendationService : IRecomendationService
	{
		private readonly IGenericRepository<Recomendation> _recomendationsRepository;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IRecomendationSettings _recomendationSettings;

		public RecomendationService(
			IGenericRepository<Recomendation> recomendationsRepository,
			INomenclatureRepository nomenclatureRepository,
			IRecomendationSettings recomendationSettings)
		{
			_recomendationsRepository = recomendationsRepository
				?? throw new ArgumentNullException(nameof(recomendationsRepository));
			_nomenclatureRepository = nomenclatureRepository
				?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_recomendationSettings = recomendationSettings
				?? throw new ArgumentNullException(nameof(recomendationSettings));
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<RecomendationItem>> GetRecomendationItemsForIpz(
			IUnitOfWork unitOfWork,
			Source source,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures,
			CancellationToken cancellationToken = default)
		{
			AvailableForSaleSourceType availableForSaleSourceType;

			switch(source)
			{
				case Source.MobileApp:
					availableForSaleSourceType = AvailableForSaleSourceType.MobileApp;
					break;
				case Source.VodovozWebSite:
					availableForSaleSourceType = AvailableForSaleSourceType.VodovozWebsite;
					break;
				case Source.KulerSaleWebSite:
					availableForSaleSourceType = AvailableForSaleSourceType.KulerServiceWebsite;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), $"Неизвестный источник {source}");
			}

			return await GetRecomendationItems(
				unitOfWork,
				availableForSaleSourceType,
				personType,
				roomType,
				excludeNomenclatures,
				_recomendationSettings.IpzCount,
				cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<RecomendationItem>> GetRecomendationItemsForRobot(
			IUnitOfWork unitOfWork,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures,
			CancellationToken cancellationToken = default) =>
			await GetRecomendationItems(
				unitOfWork,
				AvailableForSaleSourceType.RobotMia,
				personType,
				roomType,
				excludeNomenclatures,
				_recomendationSettings.RobotCount,
				cancellationToken);

		/// <inheritdoc/>
		public IEnumerable<RecomendationItem> GetRecomendationItemsForOperator(
			IUnitOfWork unitOfWork,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(
				unitOfWork,
				AvailableForSaleSourceType.WaterDelivery,
				personType,
				roomType,
				excludeNomenclatures,
				_recomendationSettings.OperatorCount)
			.GetAwaiter()
			.GetResult();

		private async Task<IEnumerable<RecomendationItem>> GetRecomendationItems(
			IUnitOfWork unitOfWork,
			AvailableForSaleSourceType source,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures,
			int limit,
			CancellationToken cancellationToken = default)
		{
			if(limit <= 0)
			{
				return Enumerable.Empty<RecomendationItem>();
			}

			var recomendationData = await GetRecomendations(
				unitOfWork,
				personType,
				roomType,
				cancellationToken);

			var recomendationItems = await GetRecomendationItems(
				unitOfWork,
				source,
				excludeNomenclatures,
				recomendationData,
				limit,
				cancellationToken);

			return recomendationItems;
		}

		private async Task<IEnumerable<RecomendationItem>> GetRecomendationItems(
			IUnitOfWork unitOfWork,
			AvailableForSaleSourceType source,
			IEnumerable<int> excludeNomenclatures,
			RecomendationData recomendationData,
			int limit,
			CancellationToken cancellationToken = default)
		{
			var allNomenclaturesInRecomendations = recomendationData
				.CommonRecomendationItems
				.Concat(recomendationData.SpecifiedRecomendationItems)
				.Concat(recomendationData.RoomTypeRecomendationItems)
				.Concat(recomendationData.PersonTypeRecomendationItems)
				.Select(x => x.NomenclatureId)
				.Distinct()
				.ToArray();

			var availableForSaleNomenclatures =
				await _nomenclatureRepository.GetAvailableForSaleNomenclatures(
					unitOfWork,
					source,
					allNomenclaturesInRecomendations,
					excludeNomenclatures,
					cancellationToken);

			var result = new List<RecomendationItem>();

			// Добавляем в первую очередь только одну наиболее приоритетную строку из общей рекомендацию
			result.AddRange(
				recomendationData.CommonRecomendationItems
				.Where(x => availableForSaleNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Take(1));

			var itemsNeeded = limit - result.Count;

			// Добавляем строки рекомендации, соответствующей типу помещения и типу КА
			result.AddRange(
				recomendationData.SpecifiedRecomendationItems
				.Where(x => availableForSaleNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded));

			itemsNeeded = limit - result.Count;

			// Добавляем строки рекомендации, соответствующей типу помещения (общая для всех типов КА)
			result.AddRange(
				recomendationData.RoomTypeRecomendationItems
				.Where(x => availableForSaleNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded));

			itemsNeeded = limit - result.Count;

			// Добавляем строки рекомендации, соответствующей типу КА (общая для всех типов помещений)
			result.AddRange(
				recomendationData.PersonTypeRecomendationItems
				.Where(x => availableForSaleNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded));

			itemsNeeded = limit - result.Count;

			var additionalCommonRecomendationItems = recomendationData.CommonRecomendationItems
				.Where(x => availableForSaleNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded)
				.OrderByDescending(x => x.Priority);

			// Если не хватило строк рекомендаций, добавляем дополнительные из общей рекомендации
			// они будут добавлены сразу после первой общей рекомендации
			foreach(var item in additionalCommonRecomendationItems)
			{
				var startIndex = result.Count == 0 ? 0 : 1;
				result.Insert(startIndex, item);
			}

			return result;
		}

		private async Task<RecomendationData> GetRecomendations(
			IUnitOfWork unitOfWork,
			PersonType? personType = null,
			RoomType? roomType = null,
			CancellationToken cancellationToken = default)
		{
			var allRecomendationsResult =
				await GetAllRecomendations(unitOfWork, personType, roomType, cancellationToken);

			if(allRecomendationsResult.IsFailure)
			{
				return new RecomendationData();
			}

			var allRecomendations = allRecomendationsResult.Value;

			var commonRecomendation =
				allRecomendations.FirstOrDefault(x => x.PersonType == null && x.RoomType == null);

			var specifiedRecomendation =
				allRecomendations.FirstOrDefault(x => x.PersonType == personType && x.RoomType == roomType);

			var roomTypeRecomendation =
				allRecomendations.FirstOrDefault(x => x.PersonType == null && x.RoomType == roomType);

			var personTypeRecomendation =
				allRecomendations.FirstOrDefault(x => x.PersonType == personType && x.RoomType == null);

			var result = new RecomendationData
			{
				CommonRecomendation = commonRecomendation,
				SpecifiedRecomendation = specifiedRecomendation,
				RoomTypeRecomendation = roomTypeRecomendation,
				PersonTypeRecomendation = personTypeRecomendation
			};

			return result;
		}

		private async Task<Result<IEnumerable<Recomendation>>> GetAllRecomendations(
			IUnitOfWork unitOfWork,
			PersonType? personType = null,
			RoomType? roomType = null,
			CancellationToken cancellationToken = default) =>
			await _recomendationsRepository
				.GetAsync(
					unitOfWork,
					x => !x.IsArchive
					&& ((x.PersonType == null && x.RoomType == null)
						|| (x.PersonType == personType && x.RoomType == roomType)
						|| (x.PersonType == personType && x.RoomType == null)
						|| (x.PersonType == null && x.RoomType == roomType)),
					cancellationToken: cancellationToken);
	}
}
