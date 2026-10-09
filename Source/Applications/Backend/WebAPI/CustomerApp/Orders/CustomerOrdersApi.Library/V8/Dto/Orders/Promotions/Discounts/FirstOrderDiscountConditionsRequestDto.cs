using System;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;

namespace CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts
{
	/// <summary>
	/// Информация для проверки доступности скидки на первый заказ для клиента
	/// </summary>
	public class FirstOrderDiscountConditionsRequestDto
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }

		/// <summary>
		/// Внешний Id пользователя
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Id пользователя в ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
	}
}
