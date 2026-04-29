using System;

namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Контракт заказа из корзины
	/// </summary>
	public interface IOnlineOrderFromCart : IFreeDeliveryPrice
	{
		/// <summary>
		/// Дата доставки
		/// </summary>
		DateTime? DeliveryDate { get; }
	}
}
