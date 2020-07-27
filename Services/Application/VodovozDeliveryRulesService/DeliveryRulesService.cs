using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesService : IDeliveryRulesService
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IDeliveryRepository deliveryRepository;

		public DeliveryRulesService(IDeliveryRepository deliveryRepository)
		{
			this.deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
		}

		public DeliveryRulesResponse GetRulesByDistrict(decimal latitude, decimal longitude)
		{
			using (var uow = UnitOfWorkFactory.CreateWithoutRoot("Получение правил доставки ")) {
				try {
					var district = deliveryRepository.GetDistrict(uow, latitude, longitude);
					if(district != null) {
						logger.Debug($"Район получен {district.DistrictName}");

						var response = new DeliveryRulesResponse {
							WeekDayDeliveryRules = new List<WeekDayDeliveryRuleDTO>(),
						};
						foreach (WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName))) {
							//Берём все правила дня недели
							var rulesToAdd = district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay).Select(x => x.Title).ToList();
							//Если правил дня недели нет берем общие правила района
							if(!rulesToAdd.Any())
								rulesToAdd = district.ObservableCommonDistrictRuleItems.Select(x => x.Title).ToList();
							var item = new WeekDayDeliveryRuleDTO {
								WeekDayEnum = weekDay,
								DeliveryRules = rulesToAdd,
								ScheduleRestrictions = district.GetScheduleRestrictionCollectionByWeekDayName(weekDay)
									.Select(x => x.DeliverySchedule)
									.OrderBy(x => x.From)
									.ThenBy(x => x.To)
									.Select(x => x.Name)
									.ToList()
							};
							response.WeekDayDeliveryRules.Add(item);
						}

						response.StatusEnum = DeliveryRulesResponseStatus.Ok;
						response.Message = "";
						return response;
					}
					
					string message = $"Невозможно получить информацию о правилах доставки так как по координатам {latitude}, {longitude} не был найден район";
					logger.Debug(message);
					return new DeliveryRulesResponse {
						StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
						WeekDayDeliveryRules = null,
						Message = message
					};
				}
				catch (Exception ex) {
					logger.Error(ex);
					return new DeliveryRulesResponse {
						StatusEnum = DeliveryRulesResponseStatus.Error,
						WeekDayDeliveryRules = null,
						Message = "Возникла внутренняя ошибка при получении правила доставки"
					};
				}
			}
		}

		public bool ServiceStatus()
		{
			var response = GetRulesByDistrict(59.886134m, 30.394007m);
			if(response.StatusEnum == DeliveryRulesResponseStatus.Error)
				return false;
			return true;
		}
	}
	
}
