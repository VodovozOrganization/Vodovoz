using DeliveryRulesService.Cache;
using DeliveryRulesService.Constants;
using DeliveryRulesService.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
using VodovozBusiness.Services.Sale;

namespace DeliveryRulesService.Controllers
{
	[Route("DeliveryRules")]
	[ApiController]
	public class DeliveryRulesController : DistrictController
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IFastDeliveryAvailabilityHistoryModel _fastDeliveryAvailabilityHistoryModel;
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
			IGeneralSettings generalSettings,
			IDistrictRulesService districtRulesService) : base(logger, deliveryRepository, districtCacheService, districtRulesService)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_deliveryRulesSettings =
				deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_fastDeliveryAvailabilityHistoryModel =
				fastDeliveryAvailabilityHistoryModel ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryModel));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

			using var uow = _uowFactory.CreateWithoutRoot("Получение графика быстрой доставки");
			_fastDeliverySchedule = uow.GetById<DeliverySchedule>(deliveryRulesSettings.FastDeliveryScheduleId);
		}

		[HttpGet("GetRulesByDistrict")]
		public async Task<(int? TariffZoneId, DeliveryRulesDTO DeliveryInfo)> GetRulesByDistrictAsync(
			[FromQuery] decimal latitude,
			[FromQuery] decimal longitude,
			CancellationToken cancellationToken)
		{
			try
			{
				return await ExecuteGetRulesByDistrictAsync(latitude, longitude, cancellationToken);
			}
			catch(Exception ex)
			{
				var errorResult = new DeliveryRulesDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.Error,
					WeekDayDeliveryRules = null,
					Message = ServiceConstants.InternalErrorFromGetDeliveryRule
				};
				Logger.LogError(ex, errorResult.Message);
				return (null, errorResult);
			}
		}


		[HttpPost("GetRulesByDistrictAndNomenclatures")]
		public async Task<DeliveryRulesDTO> GetRulesByDistrictAndNomenclatures(
			[FromBody] DeliveryRulesRequest request,
			CancellationToken cancellationToken
		)
		{
			var result = await GetRulesByDistrictAsync(request.Latitude, request.Longitude, cancellationToken);

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
			var result = await GetExtendedRulesByDistrictAsync(request.Latitude, request.Longitude, cancellationToken);
			
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
		public async Task<DeliveryInfoDTO> GetDeliveryInfo(
			[FromQuery] decimal latitude,
			[FromQuery] decimal longitude,
			CancellationToken cancellationToken)
		{
			try
			{
				return await ExecuteGetDeliveryInfoAsync(latitude, longitude, cancellationToken);
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
				Logger.LogError(ex, errorResult.Message);
				return errorResult;
			}
		}

		[HttpGet("ServiceStatus")]
		public async Task<bool> ServiceStatus(CancellationToken cancellationToken)
		{
			var response = await GetDeliveryInfo(59.886134m, 30.394007m, cancellationToken);
			var response2 = await GetRulesByDistrictAsync(59.886134m, 30.394007m, cancellationToken);
			return response.StatusEnum != DeliveryRulesResponseStatus.Error
				&& response2.DeliveryInfo.StatusEnum != DeliveryRulesResponseStatus.Error;
		}

		private async Task<(int? TariffZoneId, ExtendedDeliveryRulesDto DeliveryInfo)> GetExtendedRulesByDistrictAsync(
			decimal latitude, 
			decimal longitude,
			CancellationToken cancellationToken
			)
		{
			try
			{
				return await ExecuteGetExtendedRulesByDistrictAsync(latitude, longitude, cancellationToken);
			}
			catch(Exception ex)
			{
				var errorResult = new ExtendedDeliveryRulesDto();
				errorResult.SetErrorState();
				Logger.LogError(ex, errorResult.Message);

				return (null, errorResult);
			}
		}

		private async Task<(int? TariffZoneId, DeliveryRulesDTO DeliveyInfo)> ExecuteGetRulesByDistrictAsync(
			decimal latitude,
			decimal longitude,
			CancellationToken cancellationToken)
		{
			var date = DateTime.Now;
			Logger.LogInformation(ServiceConstants.RequestToGetDeliveryRules());

			using var uow = _uowFactory.CreateWithoutRoot();
			var district = await GetDistrictAsync(uow, latitude, longitude, cancellationToken);

			if(district != null)
			{
				Logger.LogInformation($"Район получен {district.DistrictName}");

				var response = new DeliveryRulesDTO
				{
					WeekDayDeliveryRules = new List<WeekDayDeliveryRuleDTO>(),
				};

				var isStoppedOnlineDeliveriesToday = _deliveryRulesSettings.IsStoppedOnlineDeliveriesToday;

				foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
				{
					var rulesToAdd = DistrictRulesService.GetDistrictRulesTitles(district, weekDay);

					var scheduleRestrictions =
						DistrictRulesService.GetScheduleRestrictions(district, weekDay, date, isStoppedOnlineDeliveriesToday);

					var item = new WeekDayDeliveryRuleDTO
					{
						WeekDayEnum = weekDay,
						DeliveryRules = rulesToAdd,
						ScheduleRestrictions = 
							DistrictRulesService.ReorderScheduleRestrictions(scheduleRestrictions)
								.Select(x => x.Name)
								.ToList()
					};
					response.WeekDayDeliveryRules.Add(item);
				}

				response.StatusEnum = DeliveryRulesResponseStatus.Ok;
				response.Message = "";

				return (district.TariffZone?.Id, response);
			}

			Logger.LogDebug(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude);

			var deliveryRules = new DeliveryRulesDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
					WeekDayDeliveryRules = null,
					Message = ReformatMessage(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude)
			};
			
			return (null, deliveryRules);
		}

		private async Task<(int? TariffZoneId, ExtendedDeliveryRulesDto DeliveryInfo)> ExecuteGetExtendedRulesByDistrictAsync(
			decimal latitude, 
			decimal longitude,
			CancellationToken cancellationToken
		)
		{
			var date = DateTime.Now;
			Logger.LogInformation("Поступил запрос на получение расширенных правил доставки");

			using var uow = _uowFactory.CreateWithoutRoot();
			var district = await GetDistrictAsync(uow, latitude, longitude, cancellationToken);

			if(district != null)
			{
				Logger.LogInformation("Район получен {DistrictName}", district.DistrictName);

				var response = new ExtendedDeliveryRulesDto
				{
					WeekDayDeliveryRules = new List<ExtendedWeekDayDeliveryRuleDto>()
				};

				var isStoppedOnlineDeliveriesToday = _deliveryRulesSettings.IsStoppedOnlineDeliveriesToday;
				response.PaidDeliveryId = _nomenclatureSettings.PaidDeliveryNomenclatureId;

				foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
				{
					var rulesToAdd = DistrictRulesService.GetDistrictRulesTitles(district, weekDay);

					var scheduleRestrictions = DistrictRulesService.GetScheduleRestrictions(
						district, 
						weekDay, 
						date, 
						isStoppedOnlineDeliveriesToday
					);

					var item = new ExtendedWeekDayDeliveryRuleDto
					{
						WeekDayEnum = weekDay,
						DeliveryRules = rulesToAdd,
						ScheduleRestrictions = DistrictRulesService.ReorderScheduleRestrictions(scheduleRestrictions)
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

			Logger.LogDebug("Невозможно получить информацию о правилах доставки, т.к. по координатам " +
				"{Latitude}, {Longitude} не был найден район", latitude, longitude);

			var result = new ExtendedDeliveryRulesDto();
			result.RuleNotFoundState("Невозможно получить информацию о правилах доставки, т.к. по координатам " +
				$"{latitude}, {longitude} не был найден район");

			return (null, result);
		}

		private async Task<DeliveryInfoDTO> ExecuteGetDeliveryInfoAsync(
			decimal latitude,
			decimal longitude,
			CancellationToken cancellationToken)
		{
			Logger.LogInformation(ServiceConstants.RequestToGetDeliveryRules());

			using var uow = _uowFactory.CreateWithoutRoot();
			var district = await GetDistrictAsync(uow, latitude, longitude, cancellationToken);

			if(district != null)
			{
				Logger.LogInformation($"Район получен {district.DistrictName}");
				return FillDeliveryInfoDto(district);
			}

			Logger.LogDebug(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude);

			return new DeliveryInfoDTO
			{
				StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
				WeekDayDeliveryInfos = null,
				GeoGroup = null,
				Message = ReformatMessage(ServiceConstants.DistrictNotFoundByCoordinates, latitude, longitude)
			};
		}

		private DeliveryInfoDTO FillDeliveryInfoDto(District district)
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

				var scheduleRestrictions =
					DistrictRulesService.GetScheduleRestrictions(district, weekDay, date, isStoppedOnlineDeliveriesToday);

				var item = new WeekDayDeliveryInfoDTO
				{
					DeliveryRules = rules.Any()
						? FillDeliveryRuleDto(rules) //Берём все правила дня недели
						: FillDeliveryRuleDto(district.CommonDistrictRuleItems), //Если правил дня недели нет берем общие правила района
					WeekDayEnum = weekDay,
					ScheduleRestrictions = 
						DistrictRulesService.ReorderScheduleRestrictions(scheduleRestrictions)
							.Select(x => x.Name)
							.ToList()
				};

				info.WeekDayDeliveryInfos.Add(item);
			}

			info.GeoGroup = district.GeographicGroup.Name;
			info.StatusEnum = DeliveryRulesResponseStatus.Ok;
			info.Message = "";
			return info;
		}

		private IList<DeliveryRuleDTO> FillDeliveryRuleDto<T>(IList<T> rules)
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

			var fastDeliveryAvailabilityHistory = await DeliveryRepository.GetRouteListsForFastDeliveryAsync(
				uow,
				(double)latitude,
				(double)longitude,
				isGetClosestByRoute: false,
				nomenclatureNodes,
				tariffZoneId,
				cancellationToken
			);

			fastDeliveryAvailabilityHistory.District = await DeliveryRepository.GetDistrictAsync(
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
