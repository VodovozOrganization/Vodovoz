using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <summary>
	/// Данные для расчета стоимости доставки в заказе
	/// </summary>
	public class OnlineOrderDeliveryPriceContext
	{
		/// <summary>
		/// Онлайн заказ
		/// </summary>
		public OnlineOrder OnlineOrder { get; private set; }

		public static OnlineOrderDeliveryPriceContext Create(OnlineOrder onlineOrder) =>
			new OnlineOrderDeliveryPriceContext
			{
				OnlineOrder = onlineOrder
			};
	}
}
