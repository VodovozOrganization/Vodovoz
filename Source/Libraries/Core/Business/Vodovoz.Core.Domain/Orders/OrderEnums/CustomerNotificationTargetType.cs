using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Куда ведёт пуш
	/// </summary>
	public enum CustomerNotificationTargetType
	{
		/// <summary>
		/// Страница заказа
		/// </summary>
		[Display(Name = "Страница заказа")]
		OrderInfo,

		/// <summary>
		/// Корзина
		/// </summary>
		[Display(Name = "Корзина")]
		Cart,

		/// <summary>
		/// Главная страница МП
		/// </summary>
		[Display(Name = "Главная страница МП")]
		Home
	}
}
