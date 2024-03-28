using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.Cache
{
	/// <summary>
	/// Класс для получение районов даже при отсутствии соединения с базой. Обновляет список районов каждый час
	/// </summary>
	public class DeliveryRulesCacheService
	{
		private readonly ILogger<DeliveryRulesCacheService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private int _currentActiveDistrictSetVersionId = 0;

		public DeliveryRulesCacheService(ILogger<DeliveryRulesCacheService> logger, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			Districts = new ConcurrentDictionary<int, District>();
			DistrictRuleItems = new ConcurrentDictionary<int, DistrictRuleItemBase>();
			DeliveryPriceRules = new ConcurrentDictionary<int, DeliveryPriceRule>();
			TariffZones = new ConcurrentDictionary<int, TariffZone>();
			GeoGroups = new ConcurrentDictionary<int, GeoGroup>();
		}

		public ConcurrentDictionary<int, District> Districts { get; }
		public ConcurrentDictionary<int, DistrictRuleItemBase> DistrictRuleItems { get; }
		public ConcurrentDictionary<int, DeliveryPriceRule> DeliveryPriceRules { get; }
		public ConcurrentDictionary<int, TariffZone> TariffZones { get; }
		public ConcurrentDictionary<int, GeoGroup> GeoGroups { get; }

		public void UpdateCache()
		{
			_logger.LogInformation("Обновление бэкапа районов...");

			try
			{
				RunUpdate();
				_logger.LogInformation("Обновление бэкапа районов успешно завершено");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обновлении кэша районов возникло исключение.");
			}
		}

		private void RunUpdate()
		{
			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Обновление кэша правил доставки");

			unitOfWork.Session.DefaultReadOnly = true;

			try
			{
				var currentActiveVersionId =
					(from districtSet in unitOfWork.Session.Query<DistrictsSet>()
					 where districtSet.Status == DistrictsSetStatus.Active
					 select districtSet.Id)
					.FirstOrDefault();

				UpdateGeoGroupCache(unitOfWork);
				UpdateTariffZoneCache(unitOfWork);

				if(currentActiveVersionId != _currentActiveDistrictSetVersionId)
				{
					Districts.Clear();
					_currentActiveDistrictSetVersionId = currentActiveVersionId;
				}

				UpdateDistrictCache(unitOfWork, currentActiveVersionId);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка обновления кэша районов: {ExceptionMessage}", ex.Message);
			}
		}

		private void UpdateGeoGroupCache(IUnitOfWork unitOfWork)
		{
			var geoGeroups =
				(from geoGeroup in unitOfWork.Session.Query<GeoGroup>()
					select geoGeroup)
					.ToList();

			_logger.LogInformation("Найдены следующие географические районы: {@GeoGroupPriceRules}", geoGeroups.Select(x => x.Id));
		}

		private void UpdateTariffZoneCache(IUnitOfWork unitOfWork)
		{
			var tarifZones =
				(from tarifZone in unitOfWork.Session.Query<TariffZone>()
					select tarifZone)
					.ToList();

			_logger.LogInformation("Найдены следующие тарифные зоны: {@TariffZones}", tarifZones.Select(x => x.Id));
		}

		private void UpdateDistrictCache(IUnitOfWork unitOfWork, int currentActiveVersionId)
		{
			var activeDistricts =
				(from district in unitOfWork.Session.Query<District>()
				 where district.DistrictsSet.Id == currentActiveVersionId
				 select district)
				.ToList();

			UpdateDistrictRuleCache(unitOfWork, activeDistricts
				.Select(d => d.Id)
				.Distinct()
				.ToArray());

			_logger.LogInformation("Найдены следующие активные районы: {@Districts}", activeDistricts.Select(x => x.Id));

			foreach(var district in activeDistricts)
			{
				if(Districts.ContainsKey(district.Id))
				{
					if(!Districts.TryUpdate(district.Id, district, Districts[district.Id]))
					{
						_logger.LogError("Не удалось обновить район {DistrictId} в кэше районов", district.Id);
					}
				}

				if(!Districts.TryAdd(district.Id, district))
				{
					_logger.LogError("Не удалось добавить в кэш район {DistrictId}", district.Id);
				}
			}
		}

		private void UpdateDistrictRuleCache(IUnitOfWork unitOfWork, int[] activeDistrictIds)
		{
			var districtRuleItems =
				(from districtRule in unitOfWork.Session.Query<DistrictRuleItemBase>()
				 where activeDistrictIds.Contains(districtRule.District.Id)
				 select districtRule)
				.ToList();

			var districtRuleItemsPriceRuleIds = districtRuleItems
				.Select(dr => dr.DeliveryPriceRule.Id)
				.Distinct()
				.ToArray();

			_logger.LogInformation("Найдены следующие правила доставки: {@DistrictRuleIrems}", districtRuleItems.Select(x => x.Id));

			UpdateDeliveryPriceRuleCache(unitOfWork, districtRuleItemsPriceRuleIds);
			UpdateDeliveryScheduleRestrictionCache(unitOfWork, districtRuleItemsPriceRuleIds);
		}

		private void UpdateDeliveryPriceRuleCache(IUnitOfWork unitOfWork, int[] districtRuleItemsPriceRuleIds)
		{
			var priceRules =
				(from priceRule in unitOfWork.Session.Query<DeliveryPriceRule>()
					where districtRuleItemsPriceRuleIds.Contains(priceRule.Id)
					select priceRule);

			_logger.LogInformation("Найдены следующие правила цен доставки: {@DeliveryPriceRules}", priceRules.Select(x => x.Id));
		}

		private void UpdateDeliveryScheduleRestrictionCache(IUnitOfWork unitOfWork, int[] activeDistrictIds)
		{
			var deliveryScheduleRestrictions =
				(from deliveryScheduleRestriction in unitOfWork.Session.Query<DeliveryScheduleRestriction>()
					where activeDistrictIds.Contains(deliveryScheduleRestriction.District.Id)
					select deliveryScheduleRestriction)
					.ToList();

			_logger.LogInformation("Найдены следующие графики доставки: {@DeliveryScheduleRestrictions}", deliveryScheduleRestrictions.Select(x => x.Id));
		}
	}
}
