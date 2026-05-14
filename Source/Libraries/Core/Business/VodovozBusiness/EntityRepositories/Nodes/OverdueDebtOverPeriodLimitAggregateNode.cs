using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Агрегированные данные по просроченной дебиторской задолженности (сверх установленных сроков)
	/// </summary>
	public class OverdueDebtOverPeriodLimitAggregateNode
	{
		/// <summary>
		/// Контрагент
		/// </summary>
		public Counterparty Counterparty { get; set; }

		/// <summary>
		/// Id заказа
		/// </summary>
		public IEnumerable<int> OrderIds { get; set; }

		/// <summary>
		/// Сумма просроченной задолженности
		/// </summary>
		public decimal DebtSum { get; set; }

		/// <summary>
		/// Количество дней просроченной задолженности
		/// </summary>
		public int OverdueDebtDays { get; set; }

		/// <summary>
		/// Организация
		/// </summary>
		public Organization Organization { get; set; }
	}
}
