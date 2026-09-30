namespace Vodovoz.EntityRepositories.Counterparties
{
	/// <summary>
	/// Количество выполненных заказов точки доставки и идентификатор последнего выполненного заказа.
	/// </summary>
	public class OrderFrequencyState
	{
		/// <summary>
		/// Общее количество выполненных заказов, соответствующих условиям расчёта частоты.
		/// </summary>
		public long OrderCount { get; set; }

		/// <summary>
		/// Идентификатор последнего выполненного заказа по дате доставки и идентификатору; null, если таких заказов нет.
		/// </summary>
		public int? LastCompletedOrderId { get; set; }
	}
}
