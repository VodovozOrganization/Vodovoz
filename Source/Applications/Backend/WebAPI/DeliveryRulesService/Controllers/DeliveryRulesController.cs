using DeliveryRulesService.Cache;
using DeliveryRulesService.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeliveryRulesService.Constants;
using Fias.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Models;
using Vodovoz.Services;

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
		private readonly FiasApiClientFactory _fiasApiClientFactory;
		private readonly IFiasApiClient _fiasApiClient;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider;
		private readonly FastDeliveryAvailabilityHistoryModel _fastDeliveryAvailabilityHistoryModel;
		private readonly DistrictCache _districtCache;
		private readonly DeliverySchedule _fastDeliverySchedule;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public DeliveryRulesController(
			ILogger<DeliveryRulesController> logger,
			IUnitOfWorkFactory uowFactory,
			IDeliveryRepository deliveryRepository,
			INomenclatureRepository nomenclatureRepository,
			FiasApiClientFactory fiasApiClientFactory,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			INomenclatureParametersProvider nomenclatureParametersProvider,
			FastDeliveryAvailabilityHistoryModel fastDeliveryAvailabilityHistoryModel,
			DistrictCache districtCache)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_fiasApiClientFactory = fiasApiClientFactory ?? throw new ArgumentNullException(nameof(fiasApiClientFactory));
			_deliveryRulesParametersProvider =
				deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_fastDeliveryAvailabilityHistoryModel =
				fastDeliveryAvailabilityHistoryModel ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryModel));
			_districtCache = districtCache ?? throw new ArgumentNullException(nameof(districtCache));
			_nomenclatureParametersProvider =
				nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
			_cancellationTokenSource = new CancellationTokenSource();

			_fiasApiClient = _fiasApiClientFactory.CreateClient();

			using(var uow = _uowFactory.CreateWithoutRoot("Получение графика быстрой доставки"))
			{
				_fastDeliverySchedule = uow.GetById<DeliverySchedule>(deliveryRulesParametersProvider.FastDeliveryScheduleId);
			}
		}

		[HttpGet]
		[Route("GetRulesByDistrict")]
		public DeliveryRulesDTO GetRulesByDistrict([FromQuery] decimal latitude, [FromQuery] decimal longitude)
		{
			try
			{
				var rules = ExecuteGetRulesByDistrict(latitude, longitude);
				return rules;
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
				return errorResult;
			}
		}

		private DeliveryRulesDTO ExecuteGetRulesByDistrict(decimal latitude, decimal longitude)
		{
			var date = DateTime.Now;
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
					district = _districtCache.Districts
						.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
				}

				if(district != null)
				{
					_logger.LogInformation($"Район получен {district.DistrictName}");

					var response = new DeliveryRulesDTO
					{
						WeekDayDeliveryRules = new List<WeekDayDeliveryRuleDTO>(),
					};

					var isStoppedOnlineDeliveriesToday = _deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday;

					foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
					{
						//Берём все правила дня недели
						var rulesToAdd =
							district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay).Select(x => x.Title).ToList();

						//Если правил дня недели нет берем общие правила района
						if(!rulesToAdd.Any())
						{
							rulesToAdd = district.ObservableCommonDistrictRuleItems.Select(x => x.Title).ToList();
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
					return response;
				}

				string message = ServiceConstants.DistrictNotFoundByCoordinates(latitude, longitude);
				_logger.LogDebug(message);
				return new DeliveryRulesDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
					WeekDayDeliveryRules = null,
					Message = message
				};
			}
		}
		
		private ExtendedDeliveryRulesDto ExecuteGetExtendedRulesByDistrict(decimal latitude, decimal longitude)
		{
			var date = DateTime.Now;
			_logger.LogInformation(ServiceConstants.RequestToGetDeliveryRules(extended: true));

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
				district = _districtCache.Districts
					.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
			}
			if(district != null)
			{
				_logger.LogInformation("Район получен " + district.DistrictName);
					
				var response = new ExtendedDeliveryRulesDto
				{
					WeekDayDeliveryRules = new List<ExtendedWeekDayDeliveryRuleDto>()
				};
					
				var isStoppedOnlineDeliveriesToday = _deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday;
					
				foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
				{
					var rulesToAdd =
						(from x in district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay) select x.Title).ToList();
						
					if (!rulesToAdd.Any())
					{
						rulesToAdd = district.ObservableCommonDistrictRuleItems.Select((CommonDistrictRuleItem x) => x.Title).ToList();
					}
						
					var scheduleRestrictions = GetScheduleRestrictions(district, weekDay, date, isStoppedOnlineDeliveriesToday);
						
					var item = new ExtendedWeekDayDeliveryRuleDto
					{
						WeekDayEnum = weekDay,
						DeliveryRules = rulesToAdd,
						ScheduleRestrictions = (from x in ReorderScheduleRestrictions(scheduleRestrictions)
							select new ExtendedScheduleRestrictionDto
							{
								Id = x.Id,
								ScheduleRestriction = x.Name
							}).ToList()
					};
					response.WeekDayDeliveryRules.Add(item);
				}
					
				response.StatusEnum = DeliveryRulesResponseStatus.Ok;
				response.Message = "";
				return response;
			}
				
			var message = ServiceConstants.DistrictNotFoundByCoordinates(latitude, longitude);
			_logger.LogDebug(message);
				
			return new ExtendedDeliveryRulesDto
			{
				StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
				WeekDayDeliveryRules = null,
				Message = message
			};
		}

		[HttpPost]
		[Route("GetRulesByDistrictAndNomenclatures")]
		public async Task<DeliveryRulesDTO> GetRulesByDistrictAndNomenclatures([FromBody] DeliveryRulesRequest request)
		{
			var deliveryInfo = GetRulesByDistrict(request.Latitude, request.Longitude);

			if(deliveryInfo.StatusEnum != DeliveryRulesResponseStatus.Ok)
			{
				return await ValueTask.FromResult(deliveryInfo);
			}
			
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot(ServiceConstants.CheckingFastDeliveryAvailable))
			{
				var fastDeliveryAllowed = await CheckIfFastDeliveryAllowedAsync(uow, request.Latitude, request.Longitude, request.SiteNomenclatures);

				var allowed =
					!_deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday
					&& fastDeliveryAllowed;

				if(allowed)
				{
					var todayInfo = deliveryInfo.WeekDayDeliveryRules.Single(x => x.WeekDayEnum == WeekDayName.Today);
					todayInfo.ScheduleRestrictions.Insert(0, _fastDeliverySchedule.Name);
				}
			}
			return await ValueTask.FromResult(deliveryInfo);
		}
		
		[HttpPost]
		[Route("GetExtendedRulesByDistrictAndNomenclatures")]
		public async Task<ExtendedDeliveryRulesDto> GetExtendedRulesByDistrictAndNomenclatures([FromBody] DeliveryRulesRequest request)
		{
			var deliveryInfo = GetExtendedRulesByDistrict(request.Latitude, request.Longitude);
			
			if (deliveryInfo.StatusEnum != 0)
			{
				return await ValueTask.FromResult(deliveryInfo);
			}
			
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot(ServiceConstants.CheckingFastDeliveryAvailable))
			{
				var fastDeliveryAllowed = await CheckIfFastDeliveryAllowedAsync(uow, request.Latitude, request.Longitude, request.SiteNomenclatures);
				
				if(!_deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday && fastDeliveryAllowed)
				{
					var todayInfo = deliveryInfo.WeekDayDeliveryRules.Single(x => x.WeekDayEnum == WeekDayName.Today);
					todayInfo.ScheduleRestrictions.Insert(0, new ExtendedScheduleRestrictionDto
					{
						Id = _fastDeliverySchedule.Id,
						ScheduleRestriction = _fastDeliverySchedule.Name
					});
					
					var fastDeliveryNomenclature = _nomenclatureParametersProvider.GetFastDeliveryNomenclature(uow);
					deliveryInfo.FastDeliveryPrice = fastDeliveryNomenclature.GetPrice(1);
				}
			}
			return await ValueTask.FromResult(deliveryInfo);
		}

		[HttpGet]
		[Route("GetDeliveryInfo")]
		public DeliveryInfoDTO GetDeliveryInfo([FromQuery] decimal latitude, [FromQuery] decimal longitude)
		{
			try
			{
				var deliveryInfo = ExecuteGetDeliveryInfo(latitude, longitude);
				return deliveryInfo;
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

		private DeliveryInfoDTO ExecuteGetDeliveryInfo(decimal latitude, decimal longitude)
		{
			_logger.LogInformation(ServiceConstants.RequestToGetDeliveryRules());

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
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
					district = _districtCache.Districts
						.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
				}

				if(district != null)
				{
					_logger.LogInformation($"Район получен {district.DistrictName}");

					return FillDeliveryInfoDTO(district);
				}

				var message = ServiceConstants.DistrictNotFoundByCoordinates(latitude, longitude);
				_logger.LogDebug(message);
				return new DeliveryInfoDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
					WeekDayDeliveryInfos = null,
					GeoGroup = null,
					Message = message
				};
			}
		}

		[HttpGet]
		[Route("ServiceStatus")]
		public bool ServiceStatus()
		{
			var response = GetDeliveryInfo(59.886134m, 30.394007m);
			var response2 = GetRulesByDistrict(59.886134m, 30.394007m);
			return response.StatusEnum != DeliveryRulesResponseStatus.Error && response2.StatusEnum != DeliveryRulesResponseStatus.Error;
		}
		
		private ExtendedDeliveryRulesDto GetExtendedRulesByDistrict(decimal latitude, decimal longitude)
		{
			try
			{
				return ExecuteGetExtendedRulesByDistrict(latitude, longitude);
			}
			catch (Exception ex)
			{
				var errorResult = new ExtendedDeliveryRulesDto();
				errorResult.SetErrorState();
				_logger.LogError(ex, errorResult.Message);
				return errorResult;
			}
		}

		private DeliveryInfoDTO FillDeliveryInfoDTO(District district)
		{
			var date = DateTime.Now;
			var info = new DeliveryInfoDTO
			{
				WeekDayDeliveryInfos = new List<WeekDayDeliveryInfoDTO>(),
			};

			var isStoppedOnlineDeliveriesToday = _deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday;

			foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
			{
				var rules = district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay).ToList();

				var scheduleRestrictions = GetScheduleRestrictions(district, weekDay, date, isStoppedOnlineDeliveriesToday);

				var item = new WeekDayDeliveryInfoDTO
				{
					DeliveryRules = rules.Any()
						? FillDeliveryRuleDTO(rules) //Берём все правила дня недели
						: FillDeliveryRuleDTO(district.ObservableCommonDistrictRuleItems), //Если правил дня недели нет берем общие правила района
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
			return deliverySchedules
				.OrderBy(ds => ds.From)
				.ThenBy(ds => ds.To);
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

		private async Task<bool> CheckIfFastDeliveryAllowedAsync(IUnitOfWork uow, decimal latitude, decimal longitude,
			SiteNomenclatureNode[] siteNomenclatures)
		{
			if(siteNomenclatures == null || siteNomenclatures.Any(x => x.ERPId == null || x.ERPId < 1))
			{
				return await ValueTask.FromResult(false);
			}

			var has19LWater = _nomenclatureRepository.Has19LWater(
				uow, siteNomenclatures.Select(x => x.ERPId.Value).ToArray());

			if(!has19LWater)
			{
				return await ValueTask.FromResult(false);
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

			var fastDeliveryAvailabilityHistory = _deliveryRepository.GetRouteListsForFastDelivery(
				uow,
				(double)latitude,
				(double)longitude,
				isGetClosestByRoute: false,
				_deliveryRulesParametersProvider,
				nomenclatureNodes);

			fastDeliveryAvailabilityHistory.District = _deliveryRepository.GetDistrict(uow, latitude, longitude);

			_fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistory(fastDeliveryAvailabilityHistory);

			var allowedRouteLists = fastDeliveryAvailabilityHistory.Items;
			return await ValueTask.FromResult(
				allowedRouteLists != null && allowedRouteLists.Any(x => x.IsValidToFastDelivery));
		}
	}
}
