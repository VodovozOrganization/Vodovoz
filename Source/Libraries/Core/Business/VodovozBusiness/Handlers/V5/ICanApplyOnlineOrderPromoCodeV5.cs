using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Handlers.V5
{
	public interface ICanApplyOnlineOrderPromoCodeV5
	{
		/// <summary>
		/// Время, когда пришел запрос
		/// </summary>
		DateTime Time { get; set; }
		/// <summary>
		/// Id клиента
		/// </summary>
		int CounterpartyId { get; set; }
		/// <summary>
		/// Товары онлайн заказа
		/// </summary>
		IEnumerable<IOnlineOrderedProductV5> Products { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		string PromoCode { get; set; }
		/// <summary>
		/// Источник
		/// </summary>
		Source Source { get; set; }
	}
}
