using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Collections.Concurrent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using NHibernate.Linq;
using System.Diagnostics;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;

namespace DeliveryRulesService.Cache
{
	/// <summary>
	/// Класс для получение районов даже при отсутствии соединения с базой. Обновляет список районов каждый час
	/// </summary>
	public class DistrictCacheService
	{
		private readonly ILogger<DistrictCacheService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private int _currentActiveDistrictSetVersionId = 0;

		public DistrictCacheService(ILogger<DistrictCacheService> logger, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
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
			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot();

			unitOfWork.Session.DefaultReadOnly = true;

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
					.ToList();

				var activeDistrictIds = activeDistricts
					.Select(d => d.Id)
					.Distinct()
					.ToArray();

				var districtRuleItems =
					(from districtRule in unitOfWork.Session.Query<DistrictRuleItemBase>()
					 where activeDistrictIds.Contains(districtRule.District.Id)
					 select districtRule)
					.ToList();

				var districtRuleItemsPriceRuleIds = districtRuleItems
					.Select(dr => dr.DeliveryPriceRule.Id)
					.Distinct()
					.ToArray();

				var priceRules =
					(from priceRule in unitOfWork.Session.Query<DeliveryPriceRule>()
					 where districtRuleItemsPriceRuleIds.Contains(priceRule.Id)
					 select priceRule);

				_logger.LogInformation("Найдены следующие активные районы: {@Districts}", activeDistricts.Select(ad => ad.Id));
				_logger.LogInformation("Найдены следующие правила доставки: {@DistrictRuleIrems}", districtRuleItems.Select(dri => dri.Id));
				_logger.LogInformation("Найдены следующие правила цен доставки: {@DeliveryPriceRules}", priceRules.Select(pr => pr.Id));

				var deliveryScheduleRestrictions =
					(from deliveryScheduleRestriction in unitOfWork.Session.Query<DeliveryScheduleRestriction>()
					 where activeDistrictIds.Contains(deliveryScheduleRestriction.District.Id)
					 select deliveryScheduleRestriction)
					 .ToList();

				_logger.LogInformation("Найдены следующие графики доставки: {@DeliveryScheduleRestrictions}", deliveryScheduleRestrictions);

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
		}
	}
}
