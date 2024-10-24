using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	/// <summary>
	/// Информация о распределении
	/// </summary>
	public class ManualPaymentMatchingViewModelAllocatedNode
	{
		/// <summary>
		/// Id распределения(строки платежа)
		/// </summary>
		public int PaymentItemId { get; set; }
		/// <summary>
		/// Id заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Статус заказа
		/// </summary>
		public OrderStatus OrderStatus { get; set; }
		/// <summary>
		/// Дата заказа
		/// </summary>
		public DateTime OrderDate { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		/// <summary>
		/// Распределенная сумма
		/// </summary>
		public decimal AllocatedSum { get; set; }
		/// <summary>
		/// Общая распределенная сумма(со всех платежей)
		/// </summary>
		public decimal AllAllocatedSum { get; set; }
		/// <summary>
		/// Статус оплаты заказа
		/// </summary>
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
		/// <summary>
		/// Статус распределения
		/// </summary>
		public AllocationStatus PaymentItemStatus { get; set; }
	}
}
