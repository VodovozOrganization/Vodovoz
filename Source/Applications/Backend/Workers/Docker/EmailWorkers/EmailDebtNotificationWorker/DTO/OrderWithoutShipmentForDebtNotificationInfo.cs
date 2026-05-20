using System;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace EmailDebtNotificationWorker.DTO
{
	/// <summary>
	/// Информация о счете без отгрузки на долг для уведомления о задолженности
	/// </summary>
	public class OrderWithoutShipmentForDebtNotificationInfo
	{
		/// <summary>
		/// Счёт без отгрузки на долг
		/// </summary>
		public OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt { get; set; }

		/// <summary>
		/// Дни просроченной задолженности
		/// </summary>
		public int OverdueDebtDays { get; set; }

		/// <summary>
		/// Дата самого старого заказа с задолженностью
		/// </summary>
		public DateTime OldestDebtOrderDate { get; set; }
	}
}
