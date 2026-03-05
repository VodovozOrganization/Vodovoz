using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Nodes.V5
{
	/// <summary>
	/// Данные для проверки промокода и товаров онлайн заказа
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
		public IEnumerable<IOnlineOrderedProductV5> Products { get; set; }
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
