using DeliveryRulesService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
	public class DistrictCacheService
	{
		private readonly ILogger<DistrictCacheService> _logger;
		private readonly IOptionsMonitor<DistrictCacheServiceSettings> _optionsMonitor;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private int _currentActiveDistrictSetVersionId = 0;
		private static bool _isCachingInProcess = false;

		public DistrictCacheService(
			ILogger<DistrictCacheService> logger,
			IOptionsMonitor<DistrictCacheServiceSettings> optionsMonitor,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_optionsMonitor = optionsMonitor
				?? throw new ArgumentNullException(nameof(optionsMonitor));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			Districts = new ConcurrentDictionary<int, District>();
		}

		public ConcurrentDictionary<int, District> Districts { get; }

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
			if(_isCachingInProcess)
			{
				return;
			}

			_isCachingInProcess = true;

			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot();

			unitOfWork.Session.DefaultReadOnly = true;

			var currentOptions = _optionsMonitor.CurrentValue;

			try
			{
				var currentActiveVersionId =
					(from districtSet in unitOfWork.Session.Query<DistrictsSet>()
					 where districtSet.Status == DistrictsSetStatus.Active
					 select districtSet.Id)
					.FirstOrDefault();

				var activeDistricts =
					(from district in unitOfWork.Session.Query<District>()
					 where district.DistrictsSet.Id == currentActiveVersionId
					 select district)
					.ToArray();

				var activeDistrictIds = activeDistricts
					.Select(d => d.Id)
					.Distinct()
					.ToArray();

				var districtRuleItems =
					(from districtRule in unitOfWork.Session.Query<DistrictRuleItemBase>()
					 where activeDistrictIds.Contains(districtRule.District.Id)
					 select districtRule)
					.ToArray();

				var districtRuleItemsPriceRuleIds = districtRuleItems
					.Select(dr => dr.DeliveryPriceRule.Id)
					.Distinct()
					.ToArray();

				var priceRules =
					(from priceRule in unitOfWork.Session.Query<DeliveryPriceRule>()
					 select priceRule)
					 .ToArray();

				var geoGroups =
					(from geoGroup in unitOfWork.Session.Query<GeoGroup>()
					 select geoGroup)
					 .ToArray();

				var tariffZones =
					(from tariffZone in unitOfWork.Session.Query<TariffZone>()
					select tariffZone)
					 .ToArray();

				var deliveryScheduleRestrictions =
					(from deliveryScheduleRestriction in unitOfWork.Session.Query<DeliveryScheduleRestriction>()
					 where activeDistrictIds.Contains(deliveryScheduleRestriction.District.Id)
					 select deliveryScheduleRestriction)
					 .ToList();

				if(currentOptions.PreloadLoggingLevel != PreloadLoggingLevel.None)
				{
					_logger.LogInformation(
						"Найдены следующие географические группы: {@GeoGroups}",
						geoGroups.Select(x => (x.Id, x.Name)));

					_logger.LogInformation(
						"Найдены следующие тарифные зоны: {@TarifZones}",
						tariffZones.Select(x => (x.Id, x.Name)));

					_logger.LogInformation(
						"Найдены следующие правила цен доставки: {@DeliveryPriceRules}",
						priceRules.Select(x => (x.Id, x.Title)));

					if(currentOptions.PreloadLoggingLevel == PreloadLoggingLevel.Simple)
					{
						_logger.LogInformation(
							"Найдены следующие активные районы: {@Districts}",
							activeDistricts.Select(x => (x.Id, x.DistrictName)));
					}
					else if(currentOptions.PreloadLoggingLevel == PreloadLoggingLevel.Detailed)
					{
						_logger.LogInformation(
							"Найдены следующие активные районы: {@Districts}",
							activeDistricts.Select(x => (x.Id, x.DistrictName, x.DistrictBorder)));
					}

					_logger.LogInformation(
						"Найдены следующие правила доставки: {@DistrictRuleIrems}",
						districtRuleItems.Select(x => (x.Id, x.Price, x.DeliveryPriceRule.Id, x.DeliveryPriceRule.Title)));

					_logger.LogInformation(
						"Найдены следующие графики доставки: {@DeliveryScheduleRestrictions}",
						deliveryScheduleRestrictions.Select(x => (x.Id, x.WeekDay, x.District.Id, x.District.DistrictName)));
				}

				if(currentActiveVersionId != _currentActiveDistrictSetVersionId)
				{
					Districts.Clear();
					_currentActiveDistrictSetVersionId = currentActiveVersionId;
				}

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
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка обновления кэша районов: {ExceptionMessage}", ex.Message);
			}
			finally
			{
				_isCachingInProcess = false;
			}
		}
	}
}
