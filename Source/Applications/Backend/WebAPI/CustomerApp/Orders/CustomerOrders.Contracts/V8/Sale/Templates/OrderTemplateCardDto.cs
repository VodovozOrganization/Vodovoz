using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V8.Carts;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Краткая информация о шаблоне автозаказа
	/// </summary>
	public class OrderTemplateCardDto
	{
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int OrderTemplateId { get; set; }

		/// <summary>
		/// Статус шаблона
		/// </summary>
		public TemplateStatusDto Status { get; set; }

		/// <summary>
		/// Параметры доставки
		/// </summary>
		public TemplateDeliveryScheduleDetailedDto Schedule { get; set; }

		/// <summary>
		/// Идентификатор точки доставки
		/// </summary>
		public int DeliveryPointId { get; set; }

		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string DeliveryAddress { get; set; }

		/// <summary>
		/// Количество позиций
		/// </summary>
		public int ItemsCount { get; set; }

		/// <summary>
		/// Позиции на продажу
		/// </summary>
		public IEnumerable<CartItemDto> SaleItems { get; set; }

		/// <summary>
		/// Способ оплаты
		/// </summary>
		public TemplatePaymentType PaymentMethod { get; set; }

		/// <summary>
		/// Дата следующей доставки
		/// </summary>
		public DateTime NextDeliveryDate { get; set; }

		/// <summary>
		/// Оценочная сумма
		/// </summary>
		public int EstimatedSum { get; set; }
	}
}
