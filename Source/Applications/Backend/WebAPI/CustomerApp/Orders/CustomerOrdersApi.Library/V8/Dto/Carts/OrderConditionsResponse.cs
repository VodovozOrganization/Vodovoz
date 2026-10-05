using System.Collections.Generic;
using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V8.Dto.Carts
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
		/// Условия по автозаказу
		/// </summary>
		public OnlineAutoOrderConditions AutoOrderConditions { get; set; }
		/// <summary>
		/// Информационные сообщения
		/// </summary>
		public IEnumerable<InfoMessage> InfoMessages { get; set; }

		public static OrderConditionsResponse Create(
			IEnumerable<PaymentMethod> paymentMethods,
			OnlineAutoOrderConditions onlineAutoOrder,
			DeliveryRulesConditions deliveryRulesConditions,
			IEnumerable<InfoMessage> infoMessages)
		{
			return new OrderConditionsResponse
			{
				PaymentMethods = paymentMethods,
				AutoOrderConditions = onlineAutoOrder,
				DeliveryRulesConditions = deliveryRulesConditions,
				InfoMessages = infoMessages
			};
		}
	}
}
