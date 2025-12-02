using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Nodes
{
	/// <summary>
	/// Данные для проверки промокода и товары онлайн заказа
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
		public Source Source { get; set; }
	}
}
