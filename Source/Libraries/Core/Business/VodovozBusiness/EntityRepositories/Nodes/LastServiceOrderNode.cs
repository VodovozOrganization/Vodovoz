using System;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные последнего выполненного сервисного заказа контрагента
	/// </summary>
	public class LastServiceOrderNode
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Id точки доставки, null - для самовывоза
		/// </summary>
		public int? DeliveryPointId { get; set; }

		/// <summary>
		/// Признак самовывоза
		/// </summary>
		public bool IsSelfDelivery { get; set; }

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime? DeliveryDate { get; set; }

		/// <summary>
		/// Номер для связи, указанный в заказе
		/// </summary>
		public string ContactPhoneNumber { get; set; }
	}
}
