using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DeliveryRulesService.Cache;
using DeliveryRulesService.Constants;
using DeliveryRulesService.Factories;
using DeliveryRulesService.V2.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Delivery;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Models;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders.Delivery;
using VodovozBusiness.Extensions;

namespace DeliveryRulesService.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	[ApiController]
	public class DeliveryRulesController : VersionedController
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IFastDeliveryAvailabilityHistoryModel _fastDeliveryAvailabilityHistoryModel;
		private readonly DistrictCacheService _districtCacheService;
		private readonly IDeliveryRulesHandler _deliveryRulesHandler;
		private readonly ICartItemFactory _cartItemFactory;
		private readonly IInfoMessageFactory _infoMessageFactory;
		private readonly IGeneralSettings _generalSettings;
		private readonly DeliverySchedule _fastDeliverySchedule;
		private readonly INamedDomainObject _paidDeliveryNomenclature;

		public DeliveryRulesController(
			ILogger<DeliveryRulesController> logger,
			IUnitOfWorkFactory uowFactory,
			IDeliveryRepository deliveryRepository,
			INomenclatureRepository nomenclatureRepository,
			IDeliveryRulesSettings deliveryRulesSettings,
			INomenclatureSettings nomenclatureSettings,
			IFastDeliveryAvailabilityHistoryModel fastDeliveryAvailabilityHistoryModel,
			DistrictCacheService districtCacheService,
			IDeliveryRulesHandler deliveryRulesHandler,
			ICartItemFactory cartItemFactory,
			IInfoMessageFactory infoMessageFactory,
			IGeneralSettings generalSettings) : base(logger)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_deliveryRulesSettings =
				deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_fastDeliveryAvailabilityHistoryModel =
				fastDeliveryAvailabilityHistoryModel ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryModel));
			_districtCacheService = districtCacheService ?? throw new ArgumentNullException(nameof(districtCacheService));
			_deliveryRulesHandler = deliveryRulesHandler ?? throw new ArgumentNullException(nameof(deliveryRulesHandler));
			_cartItemFactory = cartItemFactory ?? throw new ArgumentNullException(nameof(cartItemFactory));
			_infoMessageFactory = infoMessageFactory ?? throw new ArgumentNullException(nameof(infoMessageFactory));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

			using var uow = _uowFactory.CreateWithoutRoot("Получение графика быстрой доставки");
			_fastDeliverySchedule = uow.GetById<DeliverySchedule>(deliveryRulesSettings.FastDeliveryScheduleId);
			_paidDeliveryNomenclature = _nomenclatureRepository.GetNamedNomenclatureNode(uow, _nomenclatureSettings.PaidDeliveryNomenclatureId);
		}
		
		[HttpPost]
		public async Task<ExtendedDeliveryRulesDto> GetExtendedRulesByDistrictAndNomenclatures(
			[FromBody] DeliveryRulesRequest request,
			CancellationToken cancellationToken
		)
		{
			try
			{
				using var uow = _uowFactory.CreateWithoutRoot(ServiceConstants.CheckingFastDeliveryAvailable);
			
				var result = await ExecuteGetExtendedRulesByDistrict(
					request,
					cancellationToken);
			
				var deliveryInfo = result.DeliveryInfo;
			
				if(deliveryInfo.Status != DeliveryRulesResponseStatus.Ok)
				{
					return deliveryInfo;
				}

				var fastDeliveryAllowed = await CheckIfFastDeliveryAllowedAsync(
					uow, 
					request.Latitude, 
					request.Longitude, 
					result.TariffZoneId, 
					request.SaleItems,
					cancellationToken
				);

				if(!_deliveryRulesSettings.IsStoppedOnlineDeliveriesToday && fastDeliveryAllowed)
				{
					var fastDeliveryNomenclature = _nomenclatureRepository.GetFastDeliveryNomenclature(uow);

					deliveryInfo.AddFastDelivery(
						fastDeliveryNomenclature.Id,
						fastDeliveryNomenclature.GetPrice(1),
						fastDeliveryNomenclature.Name,
						_fastDeliverySchedule.Id,
						_fastDeliverySchedule.Name);
				}
			
				return result.DeliveryInfo;
			}
			catch(Exception ex)
			{
				var errorResult = new ExtendedDeliveryRulesDto();
				errorResult.SetErrorState();
				_logger.LogError(ex, errorResult.Message);
				return errorResult;
			}
		}

		[HttpGet]
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
					Status = DeliveryRulesResponseStatus.Error,
					WeekDayDeliveryRules = null,
					Message = ServiceConstants.InternalErrorFromGetDeliveryRule
				};
				_logger.LogError(ex, errorResult.Message);
				return (null, errorResult);
			}
		}

		[HttpGet]
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

		[HttpGet]
		public bool ServiceStatus()
		{
			var response = GetDeliveryInfo(59.886134m, 30.394007m);
			var response2 = GetRulesByDistrict(59.886134m, 30.394007m);
			return response.StatusEnum != DeliveryRulesResponseStatus.Error
				&& response2.DeliveryInfo.Status != DeliveryRulesResponseStatus.Error;
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

				response.Status = DeliveryRulesResponseStatus.Ok;
				response.Message = "";

				return (district.TariffZone?.Id, response);
			}

			_logger.LogDebug(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude);

			var deliveryRules = new DeliveryRulesDTO
				{
					Status = DeliveryRulesResponseStatus.RuleNotFound,
					WeekDayDeliveryRules = null,
					Message = ReformatMessage(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude)
				};
			
			return (null, deliveryRules);
		}

		private async Task<(int? TariffZoneId, ExtendedDeliveryRulesDto DeliveryInfo)> ExecuteGetExtendedRulesByDistrict(
			DeliveryRulesRequest request,
			CancellationToken cancellationToken
		)
		{
			var date = DateTime.Now;
			_logger.LogInformation("Поступил запрос на получение расширенных правил доставки");

			using var uow = _uowFactory.CreateWithoutRoot();

			District district;

			try
			{
				district = await _deliveryRepository.GetDistrictAsync(uow, request.Latitude, request.Longitude, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при подборе района по координатам");
				_logger.LogInformation("Подбор района из кэша");
				
				district = _districtCacheService.Districts.Values
					.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)request.Latitude, (double)request.Longitude)));
			}

			if(district != null)
			{
				_logger.LogInformation("Район получен {DistrictName}", district.DistrictName);

				var response = new ExtendedDeliveryRulesDto
				{
					WeekDayDeliveryRules = new List<ExtendedWeekDayDeliveryRuleDto>()
				};

				var isStoppedOnlineDeliveriesToday = _deliveryRulesSettings.IsStoppedOnlineDeliveriesToday;
				var hasPaidDelivery = false;
				var deliveryPoint = request.ErpDeliveryPointId.HasValue
					? uow.GetById<DeliveryPoint>(request.ErpDeliveryPointId.Value)
					: null;

				foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
				{
					var scheduleRestrictions = GetScheduleRestrictions(
						district, 
						weekDay, 
						date, 
						isStoppedOnlineDeliveriesToday
					);

					var deliveryPriceContext = DeliveryRulesGetterFromDeliveryRulesApiContext.Create(
							weekDay,
							district,
							deliveryPoint,
							request.IsSelfDelivery,
							request.SaleItems
								.Select(x => _cartItemFactory.CreateCartItem(uow, x))
								.ToList()
						);
					
					var paidDeliveryResult = _deliveryRulesHandler.GetDeliveryCost(deliveryPriceContext);

					if(paidDeliveryResult.IsFailure)
					{
						var errorResult = new ExtendedDeliveryRulesDto();
						errorResult.RuleNotFoundState(paidDeliveryResult.Errors.First().Message);
						
						return (null, errorResult);
					}
					
					var paidDeliveryCost = paidDeliveryResult.Value;
					
					var item = ExtendedWeekDayDeliveryRuleDto.Create(
						weekDay,
						ReorderScheduleRestrictions(scheduleRestrictions)
							.Select(x => new ScheduleRestrictionDto
							{
								ErpId = x.Id,
								IntervalName = x.Name
							})
							.ToList(),
						paidDeliveryCost.DeliveryPrice,
						_infoMessageFactory.CreatePaidDeliveryMessage(paidDeliveryCost.Message)
					);

					if(paidDeliveryCost.DeliveryPrice.HasValue)
					{
						hasPaidDelivery = true;
					}
					
					response.WeekDayDeliveryRules.Add(item);
				}

				if(hasPaidDelivery)
				{
					response.PaidDelivery = PaidDeliveryDto.Create(
						_paidDeliveryNomenclature.Id,
						_paidDeliveryNomenclature.Name
					);
				}

				response.Status = DeliveryRulesResponseStatus.Ok;
				response.Message = "";

				return (district.TariffZone?.Id, response);
			}

			_logger.LogDebug("Невозможно получить информацию о правилах доставки, т.к. по координатам " +
				"{Latitude}, {Longitude} не был найден район", request.Latitude, request.Longitude);

			var result = new ExtendedDeliveryRulesDto();
			result.RuleNotFoundState("Невозможно получить информацию о правилах доставки, т.к. по координатам " +
				$"{request.Latitude}, {request.Longitude} не был найден район");

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
			var dayOfWeek = currentDate.DayOfWeek.ConvertToWeekDayName();

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
			SaleItemDto[] saleItems,
			CancellationToken cancellationToken
			)
		{
			if(saleItems == null || saleItems.Any(x => x.ErpId is null or < 1))
			{
				return false;
			}

			if(saleItems.Any(x => !x.IsNotServiceNomenclature))
			{
				return false;
			}

			var nomenclatureNodes = saleItems
				.Select(x =>
				{
					Debug.Assert(x.ErpId != null, "x.ErpId != null");
					return new NomenclatureAmountNode
					{
						NomenclatureId = x.ErpId.Value,
						Amount = x.Amount
					};
				})
				.ToList();

			var receivedNomenclaturesIds = nomenclatureNodes.Select(x => x.NomenclatureId).ToArray();
			var nomenclatures19LWaterIds = await _nomenclatureRepository
				.Get19LWaterNomenclatureIds(uow, receivedNomenclaturesIds, cancellationToken);

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

			var matchEvaluator = new MatchEvaluator(match => "{" + argReplacement.IndexOf(match.Value) + "}");
			var result = Regex.Replace(text, "{(.*)}", matchEvaluator);

			return string.Format(result, args);
		}
	}
}
