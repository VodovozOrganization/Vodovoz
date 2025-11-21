using FluentNHibernate.Conventions;
using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Goods.Recomendations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Application.Goods
{
	internal sealed class RecomendationService : IRecomendationService
	{
		private readonly IGenericRepository<Recomendation> _recomendationsRepository;
		private readonly IGenericRepository<Nomenclature> _nomenclaturesRepository;
		private readonly IRecomendationSettings _recomendationSettings;

		public RecomendationService(
			IGenericRepository<Recomendation> recomendationsRepository,
			IGenericRepository<Nomenclature> nomenclaturesRepository,
			IRecomendationSettings recomendationSettings)
		{
			_recomendationsRepository = recomendationsRepository
				?? throw new ArgumentNullException(nameof(recomendationsRepository));
			_nomenclaturesRepository = nomenclaturesRepository
				?? throw new ArgumentNullException(nameof(nomenclaturesRepository));
			_recomendationSettings = recomendationSettings
				?? throw new ArgumentNullException(nameof(recomendationSettings));
		}

		public IEnumerable<RecomendationItem> GetRecomendationItemsForIpz(
			IUnitOfWork unitOfWork,
			Source source,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures)
		{
			switch(source)
			{
				case Source.MobileApp:
					return GetRecomendationItems(
						unitOfWork,
						SourceType.MobileApp,
						personType,
						roomType,
						excludeNomenclatures,
						_recomendationSettings.IpzCount);
				case Source.VodovozWebSite:
					return GetRecomendationItems(
						unitOfWork,
						SourceType.SiteVv,
						personType,
						roomType,
						excludeNomenclatures,
						_recomendationSettings.IpzCount);
				case Source.KulerSaleWebSite:
					return GetRecomendationItems(
						unitOfWork,
						SourceType.SiteKs,
						personType,
						roomType,
						excludeNomenclatures,
						_recomendationSettings.IpzCount);
				default:
					throw new ArgumentOutOfRangeException(nameof(source), $"Неизвестный источник {source}");
			}
		}

		public IEnumerable<RecomendationItem> GetRecomendationItemsForOperator(
			IUnitOfWork unitOfWork,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(
				unitOfWork,
				SourceType.WaterDelivery,
				personType,
				roomType,
				excludeNomenclatures,
				_recomendationSettings.OperatorCount);

		public IEnumerable<RecomendationItem> GetRecomendationItemsForRobot(
			IUnitOfWork unitOfWork,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures) =>
			GetRecomendationItems(
				unitOfWork,
				SourceType.RobotMia,
				personType,
				roomType,
				excludeNomenclatures,
				_recomendationSettings.RobotCount);

		private IEnumerable<RecomendationItem> GetRecomendationItems(
			IUnitOfWork unitOfWork,
			SourceType source,
			PersonType personType,
			RoomType roomType,
			IEnumerable<int> excludeNomenclatures,
			int limit)
		{
			if(limit <= 0)
			{
				return Enumerable.Empty<RecomendationItem>();
			}

			var recomendationData = GetRecomendations(
				unitOfWork,
				personType,
				roomType);

			var recomendationItems = GetRecomendationItems(
				unitOfWork,
				source,
				excludeNomenclatures,
				recomendationData,
				limit);

			return recomendationItems;
		}

		private IEnumerable<RecomendationItem> GetRecomendationItems(
			IUnitOfWork unitOfWork,
			SourceType source,
			IEnumerable<int> excludeNomenclatures,
			RecomendationData recomendationData,
			int limit)
		{
			var allNomenclaturesInRecomendations = recomendationData
				.CommonRecomendationItems
				.Concat(recomendationData.SpecifiedRecomendationItems)
				.Concat(recomendationData.RoomTypeRecomendationItems)
				.Concat(recomendationData.PersonTypeRecomendationItems)
				.Select(x => x.NomenclatureId)
				.Distinct()
				.ToArray();

			var validNomenclatures = GetSourceValidNomenclatures(
				unitOfWork,
				source,
				excludeNomenclatures,
				allNomenclaturesInRecomendations)
				.ToArray();

			var result = new List<RecomendationItem>();

			result.AddRange(
				recomendationData.CommonRecomendationItems
				.Where(x => validNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Take(1));

			var itemsNeeded = limit - result.Count;

			result.AddRange(
				recomendationData.SpecifiedRecomendationItems
				.Where(x => validNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded));

			itemsNeeded = limit - result.Count;

			result.AddRange(
				recomendationData.RoomTypeRecomendationItems
				.Where(x => validNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded));

			itemsNeeded = limit - result.Count;

			result.AddRange(
				recomendationData.PersonTypeRecomendationItems
				.Where(x => validNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded));

			itemsNeeded = limit - result.Count;

			var additionalCommonRecomendationItems = recomendationData.CommonRecomendationItems
				.Where(x => validNomenclatures.Any(n => n.Id == x.NomenclatureId))
				.Where(x => !result.Any(ri => ri.NomenclatureId == x.NomenclatureId))
				.Take(itemsNeeded)
				.OrderByDescending(x => x.Priority);

			foreach(var item in additionalCommonRecomendationItems)
			{
				var startIndex = result.Count == 0 ? 0 : 1;
				result.Insert(startIndex, item);
			}

			return result;
		}

		private IEnumerable<Nomenclature> GetSourceValidNomenclatures(
			IUnitOfWork unitOfWork,
			SourceType source,
			IEnumerable<int> excludeNomenclatures,
			IEnumerable<int> recomendationNomenclatureIds)
		{
			var nomenclatures =
				from n in unitOfWork.Session.Query<Nomenclature>()
				join nomenclatureOnlineParameter in unitOfWork.Session.Query<NomenclatureOnlineParameters>()
					on n.Id equals nomenclatureOnlineParameter.Nomenclature.Id into nops
				from nop in nops.DefaultIfEmpty()
				join robotMiaParameter in unitOfWork.Session.Query<RobotMiaParameters>()
					on n.Id equals robotMiaParameter.NomenclatureId into rmps
				from rmp in rmps.DefaultIfEmpty()
				where
					recomendationNomenclatureIds.Contains(n.Id)
					&& !excludeNomenclatures.Contains(n.Id)
					&& ((source == SourceType.WaterDelivery)
						|| (source == SourceType.MobileApp && nop.Type == GoodsOnlineParameterType.ForMobileApp && nop.NomenclatureOnlineAvailability == GoodsOnlineAvailability.ShowAndSale)
						|| (source == SourceType.SiteVv && nop.Type == GoodsOnlineParameterType.ForVodovozWebSite && nop.NomenclatureOnlineAvailability == GoodsOnlineAvailability.ShowAndSale)
						|| (source == SourceType.SiteKs && nop.Type == GoodsOnlineParameterType.ForKulerSaleWebSite && nop.NomenclatureOnlineAvailability == GoodsOnlineAvailability.ShowAndSale)
						|| (source == SourceType.RobotMia && rmp.GoodsOnlineAvailability == GoodsOnlineAvailability.ShowAndSale))
				select n;

			return nomenclatures.ToArray();
		}

		private RecomendationData GetRecomendations(
			IUnitOfWork unitOfWork,
			PersonType? personType = null,
			RoomType? roomType = null)
		{
			var allRecomendations = GetAllRecomendations(unitOfWork, personType, roomType);

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

		private IEnumerable<Recomendation> GetAllRecomendations(
			IUnitOfWork unitOfWork,
			PersonType? personType = null,
			RoomType? roomType = null) =>
			_recomendationsRepository
				.Get(
					unitOfWork,
					x => !x.IsArchive
					&& ((x.PersonType == null && x.RoomType == null)
						|| (x.PersonType == personType && x.RoomType == roomType)
						|| (x.PersonType == personType && x.RoomType == null)
						|| (x.PersonType == null && x.RoomType == roomType)));
	}

	public class RecomendationData
	{
		public Recomendation CommonRecomendation { get; set; }
		public Recomendation SpecifiedRecomendation { get; set; }
		public Recomendation RoomTypeRecomendation { get; set; }
		public Recomendation PersonTypeRecomendation { get; set; }

		public IEnumerable<RecomendationItem> CommonRecomendationItems =>
			CommonRecomendation?.Items?
			.OrderBy(x => x.Priority)
			.ToArray() ?? Enumerable.Empty<RecomendationItem>();

		public IEnumerable<RecomendationItem> SpecifiedRecomendationItems =>
			SpecifiedRecomendation?.Items?
			.OrderBy(x => x.Priority)
			.ToArray() ?? Enumerable.Empty<RecomendationItem>();

		public IEnumerable<RecomendationItem> RoomTypeRecomendationItems =>
			RoomTypeRecomendation?.Items?
			.OrderBy(x => x.Priority)
			.ToArray() ?? Enumerable.Empty<RecomendationItem>();

		public IEnumerable<RecomendationItem> PersonTypeRecomendationItems =>
			PersonTypeRecomendation?.Items?
			.OrderBy(x => x.Priority)
			.ToArray() ?? Enumerable.Empty<RecomendationItem>();
	}

	/// <summary>
	/// Источник запроса рекомендаций
	/// </summary>
	public enum SourceType
	{
		/// <summary>
		/// Программа доставки воды
		/// </summary>
		WaterDelivery,
		/// <summary>
		/// Клиентское мобильное приложение
		/// </summary>
		MobileApp,
		/// <summary>
		/// Сайт ВВ
		/// </summary>
		SiteVv,
		/// <summary>
		/// Сайт КС
		/// </summary>
		SiteKs,
		/// <summary>
		/// Робот Миа
		/// </summary>
		RobotMia
	}
}
