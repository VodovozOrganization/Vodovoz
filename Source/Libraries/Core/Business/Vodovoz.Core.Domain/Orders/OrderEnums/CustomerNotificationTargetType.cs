using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Куда ведёт пуш
	/// </summary>
	public enum CustomerNotificationTargetType
	{
		[Display(Name = "Страница заказа")]
		OrderInfo,

		[Display(Name = "Корзина")]
		Cart,

		[Display(Name = "Главная страница МП")]
		Home
	}
}
