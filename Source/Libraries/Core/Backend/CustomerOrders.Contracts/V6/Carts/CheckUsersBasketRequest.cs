using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoSets;

namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Запрос по проверке корзины
	/// </summary>
	public class CheckUsersBasketRequest
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }
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
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Id гео группы в ДВ для самовывоза
		/// </summary>
		public int? SelfDeliveryGeoGroupId { get; set; }
		/// <summary>
		/// Пустых бутылей 19л на возврат
		/// </summary>
		public int BottlesReturn { get; set; }
		/// <summary>
		/// Данные по сумме и скидке
		/// </summary>
		public OnlineOrderSumDto OrderSum { get; set; }
		/// <summary>
		/// Товары
		/// </summary>
		public IList<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		/// <summary>
		/// Пакеты аренды
		/// </summary>
		public IList<OnlineRentPackageDto> OnlineRentPackages { get; set; }
		/// <summary>
		/// Промонаборы
		/// </summary>
		public IList<OrderingPromoSetDto> PromoSets { get; set; }
	}
}
