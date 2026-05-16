using System;

namespace CustomerOrders.Contracts.V5.Orders.PromoCodes
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
		public Guid ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Id пользователя в ДВ
		/// </summary>
		public int? СounterpartyErpId { get; set; }
	}
}
