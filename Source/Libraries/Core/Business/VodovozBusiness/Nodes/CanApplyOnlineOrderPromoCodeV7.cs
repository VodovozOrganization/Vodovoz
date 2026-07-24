using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Interfaces.Sale;

namespace Vodovoz.Nodes
{
	/// <summary>
	/// Данные для проверки промокода и товары онлайн заказа
	/// </summary>
	public class CanApplyOnlineOrderPromoCodeV7
	{
		/// <summary>
		/// Время, когда пришел запрос
		/// </summary>
		public DateTime Time { get; set; }
		/// <summary>
		/// Id клиента
		/// </summary>
		public int? CounterpartyId { get; set; }
		/// <summary>
		/// Товары онлайн заказа
		/// </summary>
		public IEnumerable<IOrderedCartItem> Products { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		public string PromoCode { get; set; }
		/// <summary>
		/// Источник
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum => Products.Sum(x => x.CurrentSum);
	}
}
