using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;

namespace VodovozBusiness.Domain.Orders.Cart
{
	public interface IDeliveryRulesRequestContext
	{
		/// <summary>
		/// День недели
		/// </summary>
		WeekDayName WeekDay { get; }
		/// <summary>
		/// Район
		/// </summary>
		District District { get; }
		/// <summary>
		/// Точка доставки
		/// </summary>
		DeliveryPoint DeliveryPoint { get; }
		/// <summary>
		/// Список позиций корзины
		/// </summary>
		IEnumerable<ICartItem> CartItems { get; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		bool IsSelfDelivery { get; }
	}
}
