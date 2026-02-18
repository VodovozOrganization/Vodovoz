using DeliveryRulesService.Cache;
using DeliveryRulesService.Constants;
using DeliveryRulesService.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Models;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;

namespace DeliveryRulesService.Controllers
{
	[Route("DeliveryRules")]
	[ApiController]
	public class DeliveryRulesController : ControllerBase
	{
		private readonly ILogger<DeliveryRulesController> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IFastDeliveryAvailabilityHistoryModel _fastDeliveryAvailabilityHistoryModel;
		private readonly DistrictCacheService _districtCacheService;
		private readonly IGeneralSettings _generalSettings;
		private readonly DeliverySchedule _fastDeliverySchedule;

		public DeliveryRulesController(
			ILogger<DeliveryRulesController> logger,
			IUnitOfWorkFactory uowFactory,
			IDeliveryRepository deliveryRepository,
			INomenclatureRepository nomenclatureRepository,
			IDeliveryRulesSettings deliveryRulesSettings,
			INomenclatureSettings nomenclatureSettings,
			IFastDeliveryAvailabilityHistoryModel fastDeliveryAvailabilityHistoryModel,
			DistrictCacheService districtCacheService,
			IGeneralSettings generalSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_deliveryRulesSettings =
				deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_fastDeliveryAvailabilityHistoryModel =
				fastDeliveryAvailabilityHistoryModel ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryModel));
			_districtCacheService = districtCacheService ?? throw new ArgumentNullException(nameof(districtCacheService));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

			using var uow = _uowFactory.CreateWithoutRoot("Получение графика быстрой доставки");
			_fastDeliverySchedule = uow.GetById<DeliverySchedule>(deliveryRulesSettings.FastDeliveryScheduleId);
		}

		[HttpGet("GetRulesByDistrict")]
		public (int? TariffZoneId, DeliveryRulesDTO DeliveryInfo) GetRulesByDistrict([FromQuery] decimal latitude, [FromQuery] decimal longitude)
		{
			try
			{
				return ExecuteGetRulesByDistrict(latitude, longitude);
			}
			catch(Exception ex)
			{
				var errorResult = new DeliveryRulesDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.Error,
					WeekDayDeliveryRules = null,
					Message = ServiceConstants.InternalErrorFromGetDeliveryRule
				};
				_logger.LogError(ex, errorResult.Message);
				return (null, errorResult);
			}
		}


		[HttpPost("GetRulesByDistrictAndNomenclatures")]
		public async Task<DeliveryRulesDTO> GetRulesByDistrictAndNomenclatures(
			[FromBody] DeliveryRulesRequest request,
			CancellationToken cancellationToken
		)
		{
			var result = GetRulesByDistrict(request.Latitude, request.Longitude);

			if(result.DeliveryInfo.StatusEnum != DeliveryRulesResponseStatus.Ok)
			{
				return result.DeliveryInfo;
			}

			using var uow = _uowFactory.CreateWithoutRoot(ServiceConstants.CheckingFastDeliveryAvailable);

			var fastDeliveryAllowed = await CheckIfFastDeliveryAllowedAsync(
				uow,
				request.Latitude,
				request.Longitude,
				result.TariffZoneId,
				request.SiteNomenclatures,
				cancellationToken
			);

			var allowed = !_deliveryRulesSettings.IsStoppedOnlineDeliveriesToday && fastDeliveryAllowed;
			if(allowed)
			{
				var todayInfo = result.DeliveryInfo.WeekDayDeliveryRules
					.Single(x => x.WeekDayEnum == WeekDayName.Today);
				todayInfo.ScheduleRestrictions
					.Insert(0, _fastDeliverySchedule.Name);
			}

			return result.DeliveryInfo;
		}
		
		[HttpPost("GetExtendedRulesByDistrictAndNomenclatures")]
		public async Task<ExtendedDeliveryRulesDto> GetExtendedRulesByDistrictAndNomenclatures(
			[FromBody] DeliveryRulesRequest request,
			CancellationToken cancellationToken
			)
		{
			var result = await GetExtendedRulesByDistrict(request.Latitude, request.Longitude, cancellationToken);
			
			if (result.DeliveryInfo.StatusEnum != 0)
			{
				return result.DeliveryInfo;
			}

			using var uow = _uowFactory.CreateWithoutRoot(ServiceConstants.CheckingFastDeliveryAvailable);

			var fastDeliveryAllowed = await CheckIfFastDeliveryAllowedAsync(
				uow, 
				request.Latitude, 
				request.Longitude, 
				result.TariffZoneId, 
				request.SiteNomenclatures,
				cancellationToken
			);

			if(!_deliveryRulesSettings.IsStoppedOnlineDeliveriesToday && fastDeliveryAllowed)
			{
				var todayInfo = result.DeliveryInfo.WeekDayDeliveryRules.Single(x => x.WeekDayEnum == WeekDayName.Today);
				todayInfo.ScheduleRestrictions.Insert(0, new ExtendedScheduleRestrictionDto
				{
					Id = _fastDeliverySchedule.Id,
					ScheduleRestriction = _fastDeliverySchedule.Name
				});

				var fastDeliveryNomenclature = _nomenclatureRepository.GetFastDeliveryNomenclature(uow);
				result.DeliveryInfo.FastDeliveryPrice = fastDeliveryNomenclature.GetPrice(1);
				result.DeliveryInfo.FastDeliveryId = _nomenclatureSettings.FastDeliveryNomenclatureId;
			}
			
			return result.DeliveryInfo;
		}

		[HttpGet("GetDeliveryInfo")]
		public DeliveryInfoDTO GetDeliveryInfo([FromQuery] decimal latitude, [FromQuery] decimal longitude)
		{
			try
			{
				return ExecuteGetDeliveryInfo(latitude, longitude);
			}
			catch(Exception ex)
			{
				var errorResult = new DeliveryInfoDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.Error,
					WeekDayDeliveryInfos = null,
					GeoGroup = null,
					Message = ServiceConstants.InternalErrorFromGetDeliveryRule
				};
				_logger.LogError(ex, errorResult.Message);
				return errorResult;
			}
		}

		[HttpGet("ServiceStatus")]
		public bool ServiceStatus()
		{
			var response = GetDeliveryInfo(59.886134m, 30.394007m);
			var response2 = GetRulesByDistrict(59.886134m, 30.394007m);
			return response.StatusEnum != DeliveryRulesResponseStatus.Error
				&& response2.DeliveryInfo.StatusEnum != DeliveryRulesResponseStatus.Error;
		}

		private async Task<(int? TariffZoneId, ExtendedDeliveryRulesDto DeliveryInfo)> GetExtendedRulesByDistrict(
			decimal latitude, 
			decimal longitude,
			CancellationToken cancellationToken
			)
		{
			try
			{
				return await ExecuteGetExtendedRulesByDistrict(latitude, longitude, cancellationToken);
			}
			catch(Exception ex)
			{
				var errorResult = new ExtendedDeliveryRulesDto();
				errorResult.SetErrorState();
				_logger.LogError(ex, errorResult.Message);

				return (null, errorResult);
			}
		}

		private (int? TariffZoneId, DeliveryRulesDTO DeliveyInfo) ExecuteGetRulesByDistrict(decimal latitude, decimal longitude)
		{
			var date = DateTime.Now;
			_logger.LogInformation(ServiceConstants.RequestToGetDeliveryRules());

			using var uow = _uowFactory.CreateWithoutRoot();

			District district;

			try
			{
				district = _deliveryRepository.GetDistrict(uow, latitude, longitude);
			}
			catch(Exception e)
			{
				_logger.LogError(e, ServiceConstants.ErrorGetDistrictByCoordinates);
				_logger.LogInformation(ServiceConstants.GetDistrictFromCache);
				district = _districtCacheService.Districts.Values
					.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
			}

			if(district != null)
			{
				_logger.LogInformation($"Район получен {district.DistrictName}");

				var response = new DeliveryRulesDTO
				{
					WeekDayDeliveryRules = new List<WeekDayDeliveryRuleDTO>(),
				};

				var isStoppedOnlineDeliveriesToday = _deliveryRulesSettings.IsStoppedOnlineDeliveriesToday;

				foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
				{
					//Берём все правила дня недели
					var rulesToAdd =
						district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay).Select(x => x.Title).ToList();

					//Если правил дня недели нет берем общие правила района
					if(!rulesToAdd.Any())
					{
						rulesToAdd = district.CommonDistrictRuleItems.Select(x => x.Title).ToList();
					}

					var scheduleRestrictions = GetScheduleRestrictions(district, weekDay, date, isStoppedOnlineDeliveriesToday);

					var item = new WeekDayDeliveryRuleDTO
					{
						WeekDayEnum = weekDay,
						DeliveryRules = rulesToAdd,
						ScheduleRestrictions = ReorderScheduleRestrictions(scheduleRestrictions).Select(x => x.Name).ToList()
					};
					response.WeekDayDeliveryRules.Add(item);
				}

				response.StatusEnum = DeliveryRulesResponseStatus.Ok;
				response.Message = "";

				return (district.TariffZone?.Id, response);
			}

			_logger.LogDebug(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude);

			var deliveryRules = new DeliveryRulesDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
					WeekDayDeliveryRules = null,
					Message = ReformatMessage(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude)
			};
			
			return (null, deliveryRules);
		}

		private async Task<(int? TariffZoneId, ExtendedDeliveryRulesDto DeliveryInfo)> ExecuteGetExtendedRulesByDistrict(
			decimal latitude, 
			decimal longitude,
			CancellationToken cancellationToken
		)
		{
			var date = DateTime.Now;
			_logger.LogInformation("Поступил запрос на получение расширенных правил доставки");

			using var uow = _uowFactory.CreateWithoutRoot();

			District district;

			try
			{
				district = await _deliveryRepository.GetDistrictAsync(uow, latitude, longitude, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при подборе района по координатам");
				_logger.LogInformation("Подбор района из кэша");
				
				district = _districtCacheService.Districts.Values
					.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
			}

			if(district != null)
			{
				_logger.LogInformation("Район получен {DistrictName}", district.DistrictName);

				var response = new ExtendedDeliveryRulesDto
				{
					WeekDayDeliveryRules = new List<ExtendedWeekDayDeliveryRuleDto>()
				};

				var isStoppedOnlineDeliveriesToday = _deliveryRulesSettings.IsStoppedOnlineDeliveriesToday;
				response.PaidDeliveryId = _nomenclatureSettings.PaidDeliveryNomenclatureId;

				foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
				{
					var rulesToAdd = district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay)
						.Select(x => x.Title)
						.ToList();

					if(!rulesToAdd.Any())
					{
						rulesToAdd = district.CommonDistrictRuleItems
							.Select(x => x.Title)
							.ToList();
					}

					var scheduleRestrictions = GetScheduleRestrictions(
						district, 
						weekDay, 
						date, 
						isStoppedOnlineDeliveriesToday
					);

					var item = new ExtendedWeekDayDeliveryRuleDto
					{
						WeekDayEnum = weekDay,
						DeliveryRules = rulesToAdd,
						ScheduleRestrictions = ReorderScheduleRestrictions(scheduleRestrictions)
							.Select(x => new ExtendedScheduleRestrictionDto
							{
								Id = x.Id,
								ScheduleRestriction = x.Name
							})
							.ToList()
					};
					response.WeekDayDeliveryRules.Add(item);
				}

				response.StatusEnum = DeliveryRulesResponseStatus.Ok;
				response.Message = "";

				return (district.TariffZone?.Id, response);
			}

			_logger.LogDebug("Невозможно получить информацию о правилах доставки, т.к. по координатам " +
				"{Latitude}, {Longitude} не был найден район", latitude, longitude);

			var result = new ExtendedDeliveryRulesDto();
			result.RuleNotFoundState("Невозможно получить информацию о правилах доставки, т.к. по координатам " +
				$"{latitude}, {longitude} не был найден район");

			return (null, result);
		}

		private DeliveryInfoDTO ExecuteGetDeliveryInfo(decimal latitude, decimal longitude)
		{
			_logger.LogInformation(ServiceConstants.RequestToGetDeliveryRules());

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				District district;

				try
				{
					district = _deliveryRepository.GetDistrict(uow, latitude, longitude);
				}
				catch(Exception e)
				{
					_logger.LogError(e, ServiceConstants.ErrorGetDistrictByCoordinates);
					_logger.LogInformation(ServiceConstants.GetDistrictFromCache);
					district = _districtCacheService.Districts.Values
						.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
				}

				if(district != null)
				{
					_logger.LogInformation($"Район получен {district.DistrictName}");

					return FillDeliveryInfoDTO(district);
				}

				_logger.LogDebug(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude);

				return new DeliveryInfoDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
					WeekDayDeliveryInfos = null,
					GeoGroup = null,
					Message = ReformatMessage(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude)
				};
			}
		}

		private DeliveryInfoDTO FillDeliveryInfoDTO(District district)
		{
			var date = DateTime.Now;
			var info = new DeliveryInfoDTO
			{
				WeekDayDeliveryInfos = new List<WeekDayDeliveryInfoDTO>(),
			};

			var isStoppedOnlineDeliveriesToday = _deliveryRulesSettings.IsStoppedOnlineDeliveriesToday;

			foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
			{
				var rules = district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay).ToList();

				var scheduleRestrictions = GetScheduleRestrictions(district, weekDay, date, isStoppedOnlineDeliveriesToday);

				var item = new WeekDayDeliveryInfoDTO
				{
					DeliveryRules = rules.Any()
						? FillDeliveryRuleDTO(rules) //Берём все правила дня недели
						: FillDeliveryRuleDTO(district.CommonDistrictRuleItems), //Если правил дня недели нет берем общие правила района
					WeekDayEnum = weekDay,
					ScheduleRestrictions = ReorderScheduleRestrictions(scheduleRestrictions).Select(x => x.Name).ToList()
				};

				info.WeekDayDeliveryInfos.Add(item);
			}

			info.GeoGroup = district.GeographicGroup.Name;
			info.StatusEnum = DeliveryRulesResponseStatus.Ok;
			info.Message = "";
			return info;
		}

		//Сортировка по приоритетам:
		//1. Интервалы более 5 часов или ровно 5 часов, начинающиеся до 18:00
		//2. Интервалы более 5 часов или ровно 5 часов, начинающиеся после 18:00 или ровно в 18:00
		//3. Интервалы менее 5 часов, начинающиеся до 18:00
		//4. Интервалы менее 5 часов, начинающиеся после 18:00 или ровно в 18:00
		//Также сортировка в каждой группе по величине интервала и если она совпадает, по первому времени интервала
		private static IEnumerable<DeliverySchedule> ReorderScheduleRestrictions(IList<DeliverySchedule> deliverySchedules)
		{
			// Cуществует интервал с 17 до 19,  на него правила сортировки не должны действовать, этот интервал должен отображаться в самом конце в любом случае.
			// (В дальнейшем появление подобных уникальных интервалов не планируется).
			var valuesFrom17To19 = deliverySchedules.Where(x => x.From == TimeSpan.FromHours(17) && x.To == TimeSpan.FromHours(19)).ToList();

			var result = deliverySchedules
				.Where(x => x.From != TimeSpan.FromHours(17) || x.To != TimeSpan.FromHours(19))
				.OrderBy(ds => ds.From)
				.ThenByDescending(ds => ds.To)
				.ToList();

			result.AddRange(valuesFrom17To19);

			return result;
		}

		private IList<DeliveryRuleDTO> FillDeliveryRuleDTO<T>(IList<T> rules)
			where T : DistrictRuleItemBase
		{
			return rules.Select(rule => new DeliveryRuleDTO
			{
				Bottles19l = rule.DeliveryPriceRule.Water19LCount.ToString(),
				Bottles6l = rule.DeliveryPriceRule.Water6LCount.ToString(),
				Bottles1500ml = rule.DeliveryPriceRule.Water1500mlCount.ToString(),
				Bottles600ml = rule.DeliveryPriceRule.Water600mlCount.ToString(),
				Bottles500ml = rule.DeliveryPriceRule.Water500mlCount.ToString(),
				MinOrder = $"{rule.DeliveryPriceRule.OrderMinSumEShopGoods}",
				Price = $"{rule.Price:N0}"
			})
			.ToList();
		}

		private IList<DeliverySchedule> GetScheduleRestrictions(
			District district, WeekDayName weekDay, DateTime currentDate, bool isStoppedOnlineDeliveriesToday)
		{
			if(weekDay == WeekDayName.Today)
			{
				return isStoppedOnlineDeliveriesToday
					? new List<DeliverySchedule>()
					: district.GetScheduleRestrictionCollectionByWeekDayName(weekDay)
						.Where(x => x.AcceptBefore.Time > currentDate.TimeOfDay)
						.Select(x => x.DeliverySchedule)
						.ToList();
			}

			return GetScheduleRestrictionsByDate(district, weekDay, currentDate);
		}

		private IList<DeliverySchedule> GetScheduleRestrictionsForWeekDay(District district, WeekDayName weekDay)
		{
			return district
				.GetScheduleRestrictionCollectionByWeekDayName(weekDay)
				.Select(x => x.DeliverySchedule)
				.ToList();
		}

		/// <summary>
		/// Формирует список доступных интервалов доставки на день недели
		/// Если день недели - следующий день после времени запроса, то выбираем только те интервалы у которых
		/// либо не заполнено время приема до предыдущего дня заказа
		/// либо время приема до предыдущего дня больше текущего времени
		/// Иначе берем все интервалы дня недели
		/// Т.е. если запрос поступил в понедельник в 17:30, то на вторник мы отправляем интервалы у которых не заполнено время приема
		/// и где время больше 17:30
		/// Показатель следующего дня вычисляется через разность дня недели на который надо отправить интервалы и текущего дня
		/// разница между ними всегда будет равна 1, кроме случая когда текущий день - воскресение.
		/// </summary>
		/// <param name="district">Район</param>
		/// <param name="deliveryWeekDay">День недели, на который нужно отправить доступные интервалы доставки</param>
		/// <param name="currentDate">Текущее время(когда пришел запрос)</param>
		/// <returns>Список доступных интервалов доставки на день недели</returns>
		private IList<DeliverySchedule> GetScheduleRestrictionsByDate(District district, WeekDayName deliveryWeekDay, DateTime currentDate)
		{
			var dayOfWeek = District.ConvertDayOfWeekToWeekDayName(currentDate.DayOfWeek);

			if((deliveryWeekDay - dayOfWeek == 1) || (dayOfWeek == WeekDayName.Sunday && deliveryWeekDay == WeekDayName.Monday))
			{
				return district
					.GetScheduleRestrictionCollectionByWeekDayName(deliveryWeekDay)
					.Where(x => x.AcceptBefore == null || x.AcceptBefore.Time > currentDate.TimeOfDay)
					.Select(x => x.DeliverySchedule)
					.ToList();
			}

			return GetScheduleRestrictionsForWeekDay(district, deliveryWeekDay);
		}

		private async Task<bool> CheckIfFastDeliveryAllowedAsync(
			IUnitOfWork uow,
			decimal latitude,
			decimal longitude,
			int? tariffZoneId,
			SiteNomenclatureNode[] siteNomenclatures,
			CancellationToken cancellationToken
			)
		{
			if(siteNomenclatures == null || siteNomenclatures.Any(x => x.ERPId == null || x.ERPId < 1))
			{
				return false;
			}

			var nomenclatureNodes = siteNomenclatures
				.Select(x =>
				{
					Debug.Assert(x.ERPId != null, "x.ERPId != null");
					return new NomenclatureAmountNode
					{
						NomenclatureId = x.ERPId.Value,
						Amount = x.Amount
					};
				})
				.ToList();

			var siteNomenclaturesIds = nomenclatureNodes.Select(x => x.NomenclatureId).ToArray();
			var nomenclatures19LWaterIds = await _nomenclatureRepository
				.Get19LWaterNomenclatureIds(uow, siteNomenclaturesIds, cancellationToken);

			if(!nomenclatures19LWaterIds.Any())
			{
				return false;
			}

			var isFastDelivery19LBottlesLimitActive = _generalSettings.IsFastDelivery19LBottlesLimitActive;
			if(isFastDelivery19LBottlesLimitActive)
			{
				var water19LInOrderNodes = nomenclatureNodes
					.Where(n => nomenclatures19LWaterIds.Contains(n.NomenclatureId));

				var bottles19lWaterInOrderCount = water19LInOrderNodes.Sum(s => s.Amount);
				var fastDelivery19LBottlesLimitCount = _generalSettings.FastDelivery19LBottlesLimitCount;

				if(bottles19lWaterInOrderCount > fastDelivery19LBottlesLimitCount)
				{
					return false;
				}
			}

			var fastDeliveryAvailabilityHistory = await _deliveryRepository.GetRouteListsForFastDeliveryAsync(
				uow,
				(double)latitude,
				(double)longitude,
				isGetClosestByRoute: false,
				nomenclatureNodes,
				tariffZoneId,
				cancellationToken
			);

			fastDeliveryAvailabilityHistory.District = await _deliveryRepository.GetDistrictAsync(
				uow, 
				latitude, 
				longitude, 
				cancellationToken
			);

			await _fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistoryAsync(
				fastDeliveryAvailabilityHistory,
				cancellationToken
			);

			var allowedRouteLists = fastDeliveryAvailabilityHistory.Items;
			var result = allowedRouteLists != null && allowedRouteLists.Any(x => x.IsValidToFastDelivery);
			return result;
		}

		private string ReformatMessage(string text, params object[] args)
		{
			var argReplacement = new List<string>();

			var matches = Regex.Matches(text, "{(.*)}");

			foreach(Match match in matches)
			{
				if(!argReplacement.Contains(match.Value))
				{
					argReplacement.Add(match.Value);
				}
			}

			var matchEveluator = new MatchEvaluator((match) => "{" + argReplacement.IndexOf(match.Value) + "}");

			var result = Regex.Replace(text, "{(.*)}", matchEveluator);

			return string.Format(result, args);
		}
	}
}
