using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fias.Service;
using NetTopologySuite.Geometries;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Models;
using Vodovoz.Services;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesService : IDeliveryRulesService
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IBackupDistrictService _backupDistrictService;
		private readonly IFiasApiClient _fiasApiClient;
		private readonly CancellationTokenSource _cancellationTokenSource;

		private readonly DeliverySchedule _fastDeliverySchedule;

		public DeliveryRulesService(
			IDeliveryRepository deliveryRepository,
			IBackupDistrictService backupDistrictService,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IFiasApiClient fiasApiClient,
			CancellationTokenSource cancellationTokenSource)
		{
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_backupDistrictService = backupDistrictService ?? throw new ArgumentNullException(nameof(backupDistrictService));
			_deliveryRulesParametersProvider =
				deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_fiasApiClient = fiasApiClient ?? throw new ArgumentNullException(nameof(fiasApiClient));
			_cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Получение графика быстрой доставки"))
			{
				_fastDeliverySchedule = uow.GetById<DeliverySchedule>(deliveryRulesParametersProvider.FastDeliveryScheduleId);
			}
		}

		public DeliveryRulesDTO GetRulesByDistrict(decimal latitude, decimal longitude)
		{
			try
			{
				var date = DateTime.Now;
				_logger.Info("Поступил запрос на получение правил доставки");

				using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					District district;

					try
					{
						district = _deliveryRepository.GetDistrict(uow, latitude, longitude);
					}
					catch (Exception e)
					{
						_logger.Error(e, "Ошибка при подборе района по координатам");
						_logger.Info("Пробую подобрать район из бэкапа...");
						district = _backupDistrictService
							.Districts
							.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
					}

					if(district != null)
					{
						_logger.Info($"Район получен {district.DistrictName}");

						var response = new DeliveryRulesDTO
						{
							WeekDayDeliveryRules = new List<WeekDayDeliveryRuleDTO>(),
						};

						var isStoppedOnlineDeliveriesToday = _deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday;

						foreach (WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
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

							var item = new WeekDayDeliveryRuleDTO {
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

					string message = $"Невозможно получить информацию о правилах доставки так как по координатам {latitude}, {longitude} не был найден район";
					_logger.Debug(message);
					return new DeliveryRulesDTO {
						StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
						WeekDayDeliveryRules = null,
						Message = message
					};
				}
			}
			catch (Exception ex) {
				_logger.Error(ex);
				return new DeliveryRulesDTO
				{
					StatusEnum = DeliveryRulesResponseStatus.Error,
					WeekDayDeliveryRules = null,
					Message = "Возникла внутренняя ошибка при получении правила доставки"
				};
			}
		}

		public DeliveryRulesDTO GetRulesByDistrictAndNomenclatures(DeliveryRulesRequest request)
		{
			var deliveryInfo = GetRulesByDistrict(request.Latitude, request.Longitude);

			if(deliveryInfo.StatusEnum != DeliveryRulesResponseStatus.Ok)
			{
				return deliveryInfo;
			}
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Проверка на доставку за час"))
			{
				Task<bool> task = CheckIfFastDeliveryAllowedAsync(uow, request.Latitude, request.Longitude, request.SiteNomenclatures);
				task.Wait();
				var fastDeliveryAllowed = task.Result;
				
				var allowed =
					!_deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday
					&& fastDeliveryAllowed;

				if(allowed)
				{
					var todayInfo = deliveryInfo.WeekDayDeliveryRules.Single(x => x.WeekDayEnum == WeekDayName.Today);
					todayInfo.ScheduleRestrictions.Insert(0, _fastDeliverySchedule.Name);
				}
			}
			return deliveryInfo;
		}

		public DeliveryInfoDTO GetDeliveryInfo(decimal latitude, decimal longitude)
		{
			try
			{
				_logger.Info("Поступил запрос на получение правил доставки");

				using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					District district;

					try
					{
						district = _deliveryRepository.GetDistrict(uow, latitude, longitude);
					}
					catch (Exception e)
					{
						_logger.Error(e, "Ошибка при подборе района по координатам.");
						_logger.Info("Пробую подобрать район из бэкапа...");
						district = _backupDistrictService
							.Districts
							.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
					}

					if(district != null)
					{
						_logger.Info($"Район получен {district.DistrictName}");

						return FillDeliveryInfoDTO(district);
					}

					string message = $"Невозможно получить информацию о правилах доставки так как по координатам {latitude}, {longitude} не был найден район";
					_logger.Debug(message);
					return new DeliveryInfoDTO {
						StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
						WeekDayDeliveryInfos = null,
						GeoGroup = null,
						Message = message
					};
				}
			}
			catch (Exception ex) {
				_logger.Error(ex);
				return new DeliveryInfoDTO {
					StatusEnum = DeliveryRulesResponseStatus.Error,
					WeekDayDeliveryInfos = null,
					GeoGroup = null,
					Message = "Возникла внутренняя ошибка при получении правила доставки"
				};
			}
		}

		public bool ServiceStatus()
		{
			var response = GetDeliveryInfo(59.886134m, 30.394007m);
			var response2 = GetRulesByDistrict(59.886134m, 30.394007m);
			return response.StatusEnum != DeliveryRulesResponseStatus.Error && response2.StatusEnum != DeliveryRulesResponseStatus.Error;
		}

		private DeliveryInfoDTO FillDeliveryInfoDTO(District district)
		{
			var date = DateTime.Now;
			var info = new DeliveryInfoDTO
			{
				WeekDayDeliveryInfos = new List<WeekDayDeliveryInfoDTO>(),
			};

			var isStoppedOnlineDeliveriesToday = _deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday;

			foreach (WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
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
		private IEnumerable<DeliverySchedule> ReorderScheduleRestrictions(IList<DeliverySchedule> deliverySchedules)
		{
			var schedulesGroup1 = deliverySchedules
				.Where(x => x.From < new TimeSpan(18, 0, 0) && x.To - x.From >= new TimeSpan(5, 0, 0))
				.OrderByDescending(x => x.To - x.From).ThenBy(x => x.From);
			var schedulesGroup2 = deliverySchedules
				.Where(x => x.From >= new TimeSpan(18, 0, 0) && x.To - x.From >= new TimeSpan(5, 0, 0))
				.OrderByDescending(x => x.To - x.From).ThenBy(x => x.From);
			var schedulesGroup3 = deliverySchedules
				.Where(x => x.From < new TimeSpan(18, 0, 0) && x.To - x.From < new TimeSpan(5, 0, 0))
				.OrderByDescending(x => x.To - x.From).ThenBy(x => x.From);
			var schedulesGroup4 = deliverySchedules
				.Where(x => x.From >= new TimeSpan(18, 0, 0) && x.To - x.From < new TimeSpan(5, 0, 0))
				.OrderByDescending(x => x.To - x.From).ThenBy(x => x.From);

			return schedulesGroup1.Concat(schedulesGroup2).Concat(schedulesGroup3).Concat(schedulesGroup4);
		}

		private IList<DeliveryRuleDTO> FillDeliveryRuleDTO<T>(IList<T> rules)
			where T : DistrictRuleItemBase
		{
			return rules.Select(rule => new DeliveryRuleDTO
				{
					Bottles19l = rule.DeliveryPriceRule.Water19LCount.ToString(),
					Bottles6l = rule.DeliveryPriceRule.Water6LCount,
					Bottles1500ml = rule.DeliveryPriceRule.Water1500mlCount,
					Bottles600ml = rule.DeliveryPriceRule.Water600mlCount,
					Bottles500ml = rule.DeliveryPriceRule.Water500mlCount,
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
					: GetScheduleRestrictionsForWeekDay(district, weekDay);
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

			var fastDeliveryAvailabilityHistory = _deliveryRepository.GetRouteListsForFastDelivery(
				uow,
				(double) latitude,
				(double) longitude,
				isGetClosestByRoute: false,
				_deliveryRulesParametersProvider,
				nomenclatureNodes);

			// Пока убираем геокодирование адреса с сайта, т.к. не хватает лимитов по ключам яндекс api
			// fastDeliveryAvailabilityHistory.AddressWithoutDeliveryPoint = await _fiasApiClient.GetAddressByGeoCoder(latitude, longitude, _cancellationTokenSource.Token);

			fastDeliveryAvailabilityHistory.District = _deliveryRepository.GetDistrict(uow, latitude, longitude);

			var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(UnitOfWorkFactory.GetDefaultFactory);
			fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistory(fastDeliveryAvailabilityHistory);

			var allowedRouteLists = fastDeliveryAvailabilityHistory.Items;
			return allowedRouteLists != null && allowedRouteLists.Any(x => x.IsValidToFastDelivery);
		}
	}
}
