using System.Collections.Generic;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <summary>
	/// Данные для расчета доставки по запросу правил доставки
	/// </summary>
	public class DeliveryRulesRequestDeliveryPriceGetterContext : IDeliveryPriceGetterContext
	{
		/// <summary>
		/// День недели
		/// </summary>
		public WeekDayName WeekDay { get; private set; }
		/// <summary>
		/// Район
		/// </summary>
		public District District { get; private set; }
		/// <summary>
		/// Список позиций корзины
		/// </summary>
		public IEnumerable<ICartItem> CartItems { get; private set; }

		public static DeliveryRulesRequestDeliveryPriceGetterContext Create(
			WeekDayName weekDay,
			District district,
			IEnumerable<ICartItem> cartItems
		) =>
			new DeliveryRulesRequestDeliveryPriceGetterContext
			{
				WeekDay = weekDay,
				District = district,
				CartItems = cartItems
			};
	}
	
	public interface IDeliveryPriceGetterContext { }
}
