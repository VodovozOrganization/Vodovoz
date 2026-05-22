using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.Interfaces;

namespace CustomerOrders.Contracts
{
	/// <summary>
	/// Данные для проверки промокода и товаров онлайн заказа
	/// </summary>
	public class CanApplyOnlineOrderPromoCode
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
		public IEnumerable<IOnlineOrderedProduct> Products { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		public string PromoCode { get; set; }
		/// <summary>
		/// Источник
		/// </summary>
		public ExternalSource Source { get; set; }
	}
}
