using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	/// <summary>
	/// Информация для распределения платежа
	/// </summary>
	public class ManualPaymentMatchingAllocatingNode : JournalEntityNodeBase<Order>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		/// <summary>
		/// Статус заказа
		/// </summary>
		public OrderStatus OrderStatus { get; set; }
		/// <summary>
		/// Дата заказа
		/// </summary>
		public DateTime OrderDate { get; set; }
		/// <summary>
		/// Текущая сумма заказа
		/// </summary>
		public decimal ActualOrderSum { get; set; }
		/// <summary>
		/// Прошлые распределения
		/// </summary>
		public decimal LastPayments { get; set; }
		/// <summary>
		/// Прошлое распределение(для корректного расчета текущего распределения)
		/// </summary>
		public decimal OldCurrentPayment { get; set; }
		/// <summary>
		/// Текущее распределение
		/// </summary>
		public decimal CurrentPayment { get; set; }
		/// <summary>
		/// Флаг для расчета или перерасчета суммы распределения
		/// </summary>
		public bool Calculate { get; set; }
		/// <summary>
		/// Статус оплаты заказа
		/// </summary>
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
		/// <summary>
		/// Заказ Закр док
		/// </summary>
		public bool IsClosingDocumentsOrder { get; set; }
	}
}
