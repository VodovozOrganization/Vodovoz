using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	/// <summary>
	/// Контракт проверки бесплатной доставки
	/// </summary>
	public interface IFreeDeliveryPrice
	{
		/// <summary>
		/// Точка доставки
		/// </summary>
		DeliveryPoint DeliveryPoint { get; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		bool IsSelfDelivery { get; }
		/// <summary>
		/// Товары
		/// </summary>
		IEnumerable<IGoods> Goods { get; }
	}
}
