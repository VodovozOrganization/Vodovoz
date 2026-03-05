using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Orders.V4;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Nodes.V4
{
	/// <summary>
	/// Данные для проверки промокода и товаров онлайн заказа
	/// </summary>
	public class CanApplyOnlineOrderPromoCodeV4
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
		public IEnumerable<IOnlineOrderedProductV4> Products { get; set; }
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
