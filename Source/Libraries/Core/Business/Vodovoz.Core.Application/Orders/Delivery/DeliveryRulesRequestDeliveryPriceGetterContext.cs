using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <summary>
	/// Данные для расчета доставки по запросу правил доставки
	/// </summary>
	public class DeliveryRulesGetterFromDeliveryRulesApiContext : IDeliveryRulesRequestContext
	{
		/// <inheritdoc/>
		public WeekDayName WeekDay { get; private set; }
		/// <inheritdoc/>
		public District District { get; private set; }
		/// <inheritdoc/>
		public DeliveryPoint DeliveryPoint { get; private set; }
		/// <inheritdoc/>
		public bool IsSelfDelivery { get; private set; }
		/// <inheritdoc/>
		public IEnumerable<ICartItem> CartItems { get; private set; }

		public static DeliveryRulesGetterFromDeliveryRulesApiContext Create(
			WeekDayName weekDay,
			District district,
			DeliveryPoint deliveryPoint,
			bool selfDelivery,
			IEnumerable<ICartItem> cartItems
		) =>
			new DeliveryRulesGetterFromDeliveryRulesApiContext
			{
				WeekDay = weekDay,
				District = district,
				DeliveryPoint = deliveryPoint,
				IsSelfDelivery = selfDelivery,
				CartItems = cartItems
			};
	}
}
