using System;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные последнего выполненного заказа точки доставки (или самовывоза контрагента)
	/// </summary>
	public class PlannedOrderLastOrderNode
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Id точки доставки, null - для самовывоза
		/// </summary>
		public int? DeliveryPointId { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime? DeliveryDate { get; set; }

		/// <summary>
		/// Номер для связи, указанный в заказе
		/// </summary>
		public string ContactPhoneNumber { get; set; }

		/// <summary>
		/// Количество бутылей, доставленных по операции движения бутылей заказа
		/// </summary>
		public int? BottlesMovementDelivered { get; set; }

		/// <summary>
		/// Количество 19л воды в заказе по строкам заказа
		/// </summary>
		public decimal WaterBottlesCount { get; set; }
	}
}
