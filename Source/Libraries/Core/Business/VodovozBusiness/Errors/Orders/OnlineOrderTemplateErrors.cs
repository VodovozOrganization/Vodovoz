using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Orders
{
	public static class OnlineOrderTemplateErrors
	{
		public static Error CantCreateForSelfDelivery =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(CantCreateForSelfDelivery),
				"Функция автозаказа недоступна для самовывоза");
		
		public static Error CantCreateWithPromosetForNewClients =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(CantCreateWithPromosetForNewClients),
				"Промонаборы для новых клиентов не подходят под условия подключения автозаказа");
		
		public static Error CantCreateWithFreeRentPackages =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(CantCreateWithFreeRentPackages),
				"Оборудование/Пакеты аренды не подходят под условия подключения автозаказа");
		
		public static Error DeliveryPointIdIsNull =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(DeliveryPointIdIsNull),
				"Нельзя подключить автозаказ без данных по точке доставки");
		
		public static Error CounterpartyIdIsNull =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(CounterpartyIdIsNull),
				"Нельзя подключить автозаказ без данных по клиенту");
		
		public static Error DeliveryScheduleIdIsNull =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(DeliveryScheduleIdIsNull),
				"Нельзя подключить автозаказ без данных по интервалу доставки");
		
		public static Error DeliveryScheduleIsNotSuitableForSelectedWeekdays(string deliverySchedule) =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(DeliveryScheduleIdIsNull),
				"Выбранный интервал доставки {deliverySchedule} не подходит для выбранных дней поставок автозаказа");
		
		public static Error RepeatOrderIsNull =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(RepeatOrderIsNull),
				"Нельзя подключить автозаказ без данных по интервалу повторений");
		
		public static Error WeekdaysEmpty =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(WeekdaysEmpty),
				"Нельзя подключить автозаказ без данных по дням заказа");
	}
}
