using System;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Агрегированные данные по выполненным заказам точки доставки (или самовывоза контрагента)
	/// для расчета частоты заказов
	/// </summary>
	public class PlannedOrdersAggregatedNode
	{
		/// <summary>
		/// Id точки доставки, null - для самовывоза
		/// </summary>
		public int? DeliveryPointId { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Дата доставки первого выполненного заказа
		/// </summary>
		public DateTime? MinDeliveryDate { get; set; }

		/// <summary>
		/// Дата доставки последнего выполненного заказа
		/// </summary>
		public DateTime? MaxDeliveryDate { get; set; }

		/// <summary>
		/// Количество выполненных заказов
		/// </summary>
		public int OrdersCount { get; set; }
	}
}
