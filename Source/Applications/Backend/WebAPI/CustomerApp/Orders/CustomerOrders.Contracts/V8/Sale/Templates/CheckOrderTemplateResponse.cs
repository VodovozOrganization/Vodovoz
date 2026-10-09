using System.Collections.Generic;
using CustomerOrders.Contracts.V8.Orders;
using CustomerOrders.Contracts.V8.Orders.Items;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные ответа проверки параметров автозаказа
	/// </summary>
	public class CheckOrderTemplateResponse
	{
		/// <summary>
		/// Параметры доставки
		/// </summary>
		public TemplateDeliveryScheduleDetailedDto Schedule { get; set; }
		
		/// <summary>
		/// Общее количество товаров из всех позиций 
		/// </summary>
		public int ItemsCount { get; set; }
		
		/// <summary>
		/// Идентификатор точки доставки
		/// </summary>
		public int DeliveryPointId { get; set; }
		
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string DeliveryAddress { get; set; }
		
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public TemplatePaymentType PaymentMethod { get; set; }
		
		/// <summary>
		/// Позиции на продажу
		/// </summary>
		public IEnumerable<OnlineOrderItemWithDiscountDetailsDto> OnlineOrderItems { get; set; }
		
		/// <summary>
		/// Калькуляция по заказу(сумма, доставка, скидка и т.д.)
		/// </summary>
		public OnlineOrderSumDto OrderSum { get; set; }
		
		/// <summary>
		/// Следующие доставки
		/// </summary>
		public IEnumerable<TemplateNextDeliveriesDto> NextDeliveries { get; set; }
	}
}
