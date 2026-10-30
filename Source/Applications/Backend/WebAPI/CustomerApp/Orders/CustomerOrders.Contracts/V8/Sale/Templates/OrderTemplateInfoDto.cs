using System.Collections.Generic;
using CustomerOrders.Contracts.V8.Orders;
using CustomerOrders.Contracts.V8.Orders.Items;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Подробная информация об автозаказе (шаблоне)
	/// </summary>
	public class OrderTemplateInfoDto
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
		/// Описание статуса
		/// </summary>
		public string StatusDescription { get; set; }

		/// <summary>
		/// Дата создания шаблона в формате yyyy-MM-dd
		/// </summary>
		public string CreatedDate { get; set; }

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
		/// Способ оплаты
		/// </summary>
		public TemplatePaymentType PaymentMethod { get; set; }

		/// <summary>
		/// Данные карты пользователя
		/// </summary>
		public UserCardInfo RecurrentPayment { get; set; }

		/// <summary>
		/// Продаваемые позиции
		/// </summary>
		public IEnumerable<OnlineOrderItemWithDiscountDetailsDto> SaleItems { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		public OnlineOrderSumDto OrderSum { get; set; }

		/// <summary>
		/// Ближайшие доставки
		/// </summary>
		public IEnumerable<TemplateNextDeliveriesDto> NextDeliveries { get; set; }

		/// <summary>
		/// Предстоящий заказ (если есть)
		/// </summary>
		public TemplateUpcomingOrderDto UpcomingOrder { get; set; }

		/// <summary>
		/// Исходный заказ, по которому создан шаблон
		/// </summary>
		public TemplateSourceOrderDto SourceOrder { get; set; }

		/// <summary>
		/// Доступные действия
		/// </summary>
		public OrderTemplateActionsDto Actions { get; set; }
	}
}
