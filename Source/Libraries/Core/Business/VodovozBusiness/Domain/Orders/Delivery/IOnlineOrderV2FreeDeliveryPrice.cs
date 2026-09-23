using System.Collections.Generic;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	/// <summary>
	/// Данные для бесплатной доставки онлайн заказа версии 2
	/// </summary>
	public interface IOnlineOrderV2FreeDeliveryPrice : IFreeDeliveryPrice
	{
		/// <summary>
		/// Онлайн промо наборы
		/// </summary>
		IEnumerable<OnlineOrderPromoSet> OnlinePromoSets { get; }
	}
}
