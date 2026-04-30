using System.Collections;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Суммарные данные по просроченной дебиторской задолженности контрагента
	/// </summary>
	public class CounterpartyOverdueDebtorDebtAggregatedNode
	{
		/// <summary>
		/// Номера заказов, по которым есть просроченная дебиторская задолженность
		/// </summary>
		public IEnumerable<int> OrderIds { get; set; }

		/// <summary>
		/// Контрагент
		/// </summary>
		public Counterparty Counterparty { get; set; }

		/// <summary>
		/// Id организации
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Суммарная просроченная дебиторская задолженность
		/// </summary>
		public decimal TotalOverdueDebtorDebt { get; set; }
	}
}
