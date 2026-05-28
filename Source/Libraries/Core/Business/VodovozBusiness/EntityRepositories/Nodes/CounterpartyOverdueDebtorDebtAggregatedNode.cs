using System.Collections;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

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
		/// Полное наименование организации
		/// </summary>
		public string OrganizationFullName { get; set; }

		/// <summary>
		/// Адрес электронной почты организации
		/// </summary>
		public string OrganizationEmailForMailing { get; set; }

		/// <summary>
		/// Договор
		/// </summary>
		public CounterpartyContract Contract { get; set; }

		/// <summary>
		/// Суммарная просроченная дебиторская задолженность
		/// </summary>
		public decimal TotalOverdueDebtorDebt { get; set; }
	}
}
