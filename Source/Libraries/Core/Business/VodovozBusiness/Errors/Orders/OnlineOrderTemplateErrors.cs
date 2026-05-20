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
		
		public static Error IsEmptyDeliveryPoint =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(IsEmptyDeliveryPoint),
				"Нельзя подключить автозаказ без данных по точке доставки");
		
		public static Error CantCreateForPaidOnline =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(CantCreateForPaidOnline),
				"Функция автозаказа недоступна для оплаты онлайн");
		
		public static Error IsEmptyCounterparty =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(IsEmptyCounterparty),
				"Нельзя подключить автозаказ без данных по клиенту");
		
		public static Error IsEmptyDeliverySchedule =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(IsEmptyDeliverySchedule),
				"Нельзя подключить автозаказ без данных по интервалу доставки");
		
		public static Error DeliveryScheduleIsNotSuitableForSelectedWeekdays(string deliverySchedule) =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(DeliveryScheduleIsNotSuitableForSelectedWeekdays),
				$"Выбранный интервал доставки {deliverySchedule} не подходит для выбранных дней поставок автозаказа");
		
		public static Error IsEmptyDeliveryFrequency =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(IsEmptyDeliveryFrequency),
				"Нельзя подключить автозаказ без данных по периодичности доставки");
		
		public static Error IsEmptyWeekdays =>
			new Error(
				typeof(OnlineOrderTemplateErrors),
				nameof(IsEmptyWeekdays),
				"Нельзя подключить автозаказ без данных по дням заказа");
	}
}
