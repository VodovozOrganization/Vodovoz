using System.Collections.Generic;
using CustomerOrders.Contracts.InfoMessages;

namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Ответ с формами оплат и доп условиями к заказу из корзины
	/// </summary>
	public sealed class OrderConditionsResponse
	{
		/// <summary>
		/// Формы оплат
		/// </summary>
		public IEnumerable<PaymentMethod> PaymentMethods { get; set; }
		/// <summary>
		/// Доп условия
		/// </summary>
		public DeliveryRulesConditions DeliveryRulesConditions { get; set; }
		/// <summary>
		/// Информационные сообщения
		/// </summary>
		public IEnumerable<InfoMessage> InfoMessages { get; set; }

		public static OrderConditionsResponse Create(
			IEnumerable<PaymentMethod> paymentMethods,
			DeliveryRulesConditions deliveryRulesConditions,
			IEnumerable<InfoMessage> infoMessages)
		{
			return new OrderConditionsResponse
			{
				PaymentMethods = paymentMethods,
				DeliveryRulesConditions = deliveryRulesConditions,
				InfoMessages = infoMessages
			};
		}
	}
}
