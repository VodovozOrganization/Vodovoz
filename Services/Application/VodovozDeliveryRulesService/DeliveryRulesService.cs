using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Services;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesService : IDeliveryRulesService
	{
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;

		public DeliveryRulesService(
			IDeliveryRepository deliveryRepository,
			IBackupDistrictService backupDistrictService,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider)
		{
			this.deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			this.backupDistrictService = backupDistrictService ?? throw new ArgumentNullException(nameof(backupDistrictService));
			_deliveryRulesParametersProvider = 
				deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
		}
		
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IDeliveryRepository deliveryRepository;
		private readonly IBackupDistrictService backupDistrictService;

		public DeliveryRulesDTO GetRulesByDistrict(decimal latitude, decimal longitude)
		{
			try 
			{
				logger.Info("Поступил запрос на получение правил доставки");
				
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) 
				{
					District district;

					try 
					{
						district = deliveryRepository.GetDistrict(uow, latitude, longitude);
					}
					catch (Exception e) 
					{
						logger.Error(e, "Ошибка при подборе района по координатам");
						logger.Info("Пробую подобрать район из бэкапа...");
						district = backupDistrictService
							.Districts
							.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
					}
					
					if(district != null) 
					{
						logger.Info($"Район получен {district.DistrictName}");

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

							List<DeliverySchedule> scheduleRestrictions;
							if(weekDay == WeekDayName.Today && isStoppedOnlineDeliveriesToday)
							{
								scheduleRestrictions = new List<DeliverySchedule>();
							}
							else
							{
								scheduleRestrictions = district
									.GetScheduleRestrictionCollectionByWeekDayName(weekDay)
									.Select(x => x.DeliverySchedule)
									.ToList();
							}

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
					logger.Debug(message);
					return new DeliveryRulesDTO {
						StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
						WeekDayDeliveryRules = null,
						Message = message
					};
				}
			}
			catch (Exception ex) {
				logger.Error(ex);
				return new DeliveryRulesDTO 
				{
					StatusEnum = DeliveryRulesResponseStatus.Error,
					WeekDayDeliveryRules = null,
					Message = "Возникла внутренняя ошибка при получении правила доставки"
				};
			}
		}
		
		public DeliveryInfoDTO GetDeliveryInfo(decimal latitude, decimal longitude)
		{
			try 
			{
				logger.Info("Поступил запрос на получение правил доставки");
				
				using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) 
				{
					District district;
					
					try 
					{
						district = deliveryRepository.GetDistrict(uow, latitude, longitude);
					}
					catch (Exception e) 
					{
						logger.Error(e, "Ошибка при подборе района по координатам.");
						logger.Info("Пробую подобрать район из бэкапа...");
						district = backupDistrictService
							.Districts
							.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
					}
					
					if(district != null) 
					{
						logger.Info($"Район получен {district.DistrictName}");

						return FillDeliveryInfoDTO(district);
					}
					
					string message = $"Невозможно получить информацию о правилах доставки так как по координатам {latitude}, {longitude} не был найден район";
					logger.Debug(message);
					return new DeliveryInfoDTO {
						StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
						WeekDayDeliveryInfos = null,
						GeoGroup = null,
						Message = message
					};
				}
			}
			catch (Exception ex) {
				logger.Error(ex);
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
			if(response.StatusEnum == DeliveryRulesResponseStatus.Error || response2.StatusEnum == DeliveryRulesResponseStatus.Error)
				return false;
			return true;
		}

		private DeliveryInfoDTO FillDeliveryInfoDTO(District district)
		{
			var info = new DeliveryInfoDTO
			{
				WeekDayDeliveryInfos = new List<WeekDayDeliveryInfoDTO>(),
			};
			
			var isStoppedOnlineDeliveriesToday = _deliveryRulesParametersProvider.IsStoppedOnlineDeliveriesToday;
			
			foreach (WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName))) 
			{
				var rules = district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay).ToList();

				List<DeliverySchedule> scheduleRestrictions;
				if(weekDay == WeekDayName.Today && isStoppedOnlineDeliveriesToday)
				{
					scheduleRestrictions = new List<DeliverySchedule>();
				}
				else
				{
					scheduleRestrictions = district
						.GetScheduleRestrictionCollectionByWeekDayName(weekDay)
						.Select(x => x.DeliverySchedule)
						.ToList();
				}
				
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
	}
}
