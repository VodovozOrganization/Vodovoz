 using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Counterparties
{
	/// <summary>
	/// Количество выполненных заказов точки доставки и статус её последнего заказа.
	/// </summary>
	public class OrderFrequencyState
	{
		/// <summary>
		/// Общее количество выполненных заказов, соответствующих условиям расчёта частоты.
		/// </summary>
		public long OrderCount { get; set; }

		/// <summary>
		/// Статус последнего заказа по дате доставки и идентификатору; null, если заказов нет.
		/// </summary>
		public OrderStatus? LastOrderStatus { get; set; }
	}
}
