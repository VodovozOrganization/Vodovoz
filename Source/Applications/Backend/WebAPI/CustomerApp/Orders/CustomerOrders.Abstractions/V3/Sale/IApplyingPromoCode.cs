using System;
using System.Collections.Generic;
using CustomerApp.Contracts.Common;

namespace CustomerOrders.Abstractions.V3.Sale
{
	public interface IApplyingPromoCode
	{
		/// <summary>
		/// Источник
		/// </summary>
		ExternalSource Source { get; }
		/// <summary>
		/// Время, когда пришел запрос
		/// </summary>
		DateTime RequestTime { get; }
		/// <summary>
		/// Id клиента
		/// </summary>
		int? ErpCounterpartyId { get; }
		/// <summary>
		/// Товары онлайн заказа
		/// </summary>
		IEnumerable<IOnlineOrderedProduct> OnlineOrderItems { get; }
		/// <summary>
		/// Промокод
		/// </summary>
		string PromoCode { get; }
	}
}
