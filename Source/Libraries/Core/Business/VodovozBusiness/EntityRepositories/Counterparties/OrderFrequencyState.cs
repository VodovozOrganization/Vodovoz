using System;

namespace Vodovoz.EntityRepositories.Counterparties
{
	/// <summary>
	/// Состояние выполненного заказа, участвующего в расчёте частоты точки доставки.
	/// Переходы между выполненными статусами не изменяют состояние для расчёта.
	/// </summary>
	public class OrderFrequencyState
	{
		/// <summary>
		/// Идентификатор заказа.
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Дата доставки, используемая для отбора заказов и расчёта интервалов.
		/// </summary>
		public DateTime DeliveryDate { get; set; }
	}
}
