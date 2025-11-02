using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(
		Nominative = "Статус заказа",
		NominativePlural = "Статусы заказов",
		GenitivePlural = "Статусов заказов"
		)]
	public enum OrderStatus
	{
		[Display(Name = "Отменён")]
		Canceled,

		[Display (Name = "Новый")]
		NewOrder,

		[Display (Name = "Ожидание оплаты")]
		WaitForPayment,

		[Display (Name = "Принят")]
		Accepted,

		[Display (Name = "В маршрутном листе")]
		InTravelList,

		[Display (Name = "На погрузке")]
		OnLoading,

		[Display (Name = "В пути")]
		OnTheWay,

		[Display (Name = "Доставка отменена")]
		DeliveryCanceled,

		[Display (Name = "Доставлен")]
		Shipped,

		[Display (Name = "Выгрузка на складе")]
		UnloadingOnStock,

		[Display (Name = "Недовоз")]
		NotDelivered,

		[Display (Name = "Закрыт")]
		Closed,
	}
}
