using System;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные заказа, созданного клиентом,
	/// для поиска планируемых заказов, по которым нужно обновить сделку в Битрикс24
	/// </summary>
	public class PlannedOrderCreatedOrderNode
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
		/// Признак самовывоза
		/// </summary>
		public bool IsSelfDelivery { get; set; }

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
	}
}
