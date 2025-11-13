using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Infrastructure.Print
{
	[Flags]
	public enum RouteListPrintableDocuments
	{
		[Display(Name = "Все")]
		All,
		[Display(Name = "Маршрутный лист")]
		RouteList,
		[Display(Name = "Карта маршрута")]
		RouteMap,
		[Display(Name = "Адреса по ежедневным номерам")]
		DailyList,
		[Display(Name = "Лист времени")]
		TimeList,
		[Display(Name = "Отчёт по порядку адресов")]
		OrderOfAddresses,
		[Display(Name = "Экспедиторская расписка")]
		ForwarderReceipt,
		[Display(Name = "Уведомление о сетевом заказе")]
		ChainStoreNotification

	}
}
