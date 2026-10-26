using System;
using CustomerOrders.Contracts.V8.Common;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные результата создания шаблона автозаказа
	/// </summary>
	public class CreateOrderTemplateResponse
	{
		/// <summary>
		/// ID шаблона автозаказа
		/// </summary>
		public int OrderTemplateId { get; set; }

		/// <summary>
		/// ID онлайн-заказа
		/// </summary>
		public int OnlineOrderId { get; set; }

		/// <summary>
		/// Статус заказа
		/// </summary>
		public TemplateStatusDto Status { get; set; }

		/// <summary>
		/// Дата следующей доставки
		/// </summary>
		public DateTime? NextDeliveryDate { get; set; }

		/// <summary>
		/// Способ оплаты
		/// </summary>
		public TemplatePaymentType PaymentMethod { get; protected set; }

		/// <summary>
		/// Требуется ли оплата
		/// </summary>
		public bool IsPaymentRequired { get; set; }

		/// <summary>
		/// Метка для оплаты автозаказа <see cref="OnlineOrderTimerSetupDto"/>
		/// </summary>
		public OnlineOrderTimerSetupDto TimerForPaySeconds { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		
		public void SetPaymentMethod(
			TemplatePaymentType paymentMethod,
			OnlineOrderTimerSetupDto timeStamp)
		{
			PaymentMethod = paymentMethod;
			IsPaymentRequired = paymentMethod == TemplatePaymentType.Online || paymentMethod == TemplatePaymentType.Sbp;

			if(IsPaymentRequired)
			{
				TimerForPaySeconds = timeStamp;
			}
		}
	}
}
