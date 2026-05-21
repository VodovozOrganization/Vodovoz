using System;
using System.Collections.Generic;
<<<<<<<< HEAD:Source/Libraries/Core/Backend/CustomerOrders.Contracts/CanApplyOnlineOrderPromoCode.cs
using CustomerOrders.Contracts.Interfaces;

namespace CustomerOrders.Contracts
========
using Vodovoz.Core.Data.V4;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Nodes.V4
>>>>>>>> origin/5696_AddCreatingOnlineOrderFromTemplate:Source/Libraries/Core/Business/VodovozBusiness/Nodes/V4/CanApplyOnlineOrderPromoCodeV4.cs
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
		public ExternalSource Source { get; set; }
	}
}
