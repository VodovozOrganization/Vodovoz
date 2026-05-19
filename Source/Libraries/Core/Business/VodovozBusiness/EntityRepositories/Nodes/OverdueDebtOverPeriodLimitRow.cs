using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Строка с данными по просроченной дебиторской задолженности (сверх установленных сроков)
	/// </summary>
	public class OverdueDebtOverPeriodLimitRow
	{
		/// <summary>
		/// Клиент
		/// </summary>
		public Counterparty Counterparty { get; set; }

		/// <summary>
		/// Заказ
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Организация
		/// </summary>
		public Organization Organization { get; set; }

		/// <summary>
		/// Долг
		/// </summary>
		public decimal DebtSum { get; set; }
		/// </summary>

		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime DeliveryDate { get; set; }
	}
}
