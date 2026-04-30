using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.OrderItem;

namespace CustomerOrders.Contracts.V5.Orders.PromoCodes
{
	/// <summary>
	/// Данные для проверки промокода и товары онлайн заказа
	/// </summary>
	public class CanApplyOnlineOrderPromoCodeV5
	{
		/// <summary>
		/// Время, когда пришел запрос
		/// </summary>
		public DateTime Time { get; set; }
		/// <summary>
		/// Id клиента
		/// </summary>
		public int CounterpartyId { get; set; }
		/// <summary>
		/// Товары онлайн заказа
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> Products { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		public string PromoCode { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		/// <summary>
		/// Источник
		/// </summary>
		public ExternalSource Source { get; set; }
	}
}
