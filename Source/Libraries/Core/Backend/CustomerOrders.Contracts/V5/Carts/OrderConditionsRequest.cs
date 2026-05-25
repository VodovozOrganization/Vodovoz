using System;

namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Запрос условий доставки
	/// </summary>
	public sealed class OrderConditionsRequest
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Идентификатор проверки
		/// </summary>
		public Guid? CheckId { get; set; }
		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		public int? CounterpartyErpId { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Id точки доставки в ДВ
		/// </summary>
		public int? DeliveryPointId { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Id гео группы в ДВ для самовывоза
		/// </summary>
		public int? SelfDeliveryGeoGroupId { get; set; }
	}
}
