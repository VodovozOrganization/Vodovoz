using System.Collections.Generic;

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
		/// Id клиента
		/// </summary>
		public int CounterpartyId { get; set; }

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
		/// Идентификатор договора
		/// </summary>
		public int Contractd { get; set; }

		/// <summary>
		/// Суммарная просроченная дебиторская задолженность
		/// </summary>
		public decimal TotalOverdueDebtorDebt { get; set; }
	}
}
