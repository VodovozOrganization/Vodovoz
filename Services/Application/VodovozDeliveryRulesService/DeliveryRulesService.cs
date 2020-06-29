using System;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Delivery;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesService : IDeliveryRulesService
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IDeliveryRepository deliveryRepository;

		public DeliveryRulesService(IDeliveryRepository deliveryRepository)
		{
			this.deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
		}

		public DeliveryRulesResponse GetRulesByDistrict(decimal latitude, decimal longitude)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"Получение правил доставки ")) {
				try {
					var district = deliveryRepository.GetDistrict(uow, latitude, longitude);

					if(district != null) {
						logger.Debug($"Район получен {district.DistrictName}");
						var rule = new DeliveryRuleDTO();
						rule.MinBottles = district?.MinBottles ?? 0;
						rule.DeliveryPrice = district.CommonDistrictRuleItems.Count > 0
							? district.CommonDistrictRuleItems[0]?.Price ?? 0
							: 0;
						rule.DeliveryRuleTitle = district.CommonDistrictRuleItems.Count > 0
							? district.CommonDistrictRuleItems[0]?.DeliveryPriceRule.ToString()
							: "";
						rule.DeliverySchedule = district.GetSchedulesString();

						return new DeliveryRulesResponse {
							StatusEnum = DeliveryRulesResponseStatus.Ok,
							DeliveryRule = rule,
							Message = ""
						};
					} else {
						string message = $"Невозможно получить информацию о правилах доставки так как по координатам {latitude}, {longitude} не был найден район";
						logger.Debug(message);
						return new DeliveryRulesResponse {
							StatusEnum = DeliveryRulesResponseStatus.RuleNotFound,
							DeliveryRule = null,
							Message = message
						};
					}
				}
				catch(Exception e) {
					logger.Error(e);
					return new DeliveryRulesResponse {
						StatusEnum = DeliveryRulesResponseStatus.Error,
						DeliveryRule = null,
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
