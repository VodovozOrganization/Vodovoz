using System;

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
		public OnlineOrderStatus Status { get; set; }

		/// <summary>
		/// Дата следующей доставки
		/// </summary>
		public DateTime? NextDeliveryDate { get; set; }

		/// <summary>
		/// Способ оплаты
		/// </summary>
		public TemplatePaymentType PaymentMethod { get; set; }

		/// <summary>
		/// Требуется ли оплата
		/// </summary>
		public bool IsPaymentRequired { get; set; }

		/// <summary>
		/// Таймер до истечения времени на оплату (в секундах)
		/// </summary>
		public int TimerForPaySeconds { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
	}
}
