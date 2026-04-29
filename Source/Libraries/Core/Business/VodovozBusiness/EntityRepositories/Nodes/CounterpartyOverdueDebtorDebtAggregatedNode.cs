namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Суммарные данные по просроченной дебиторской задолженности контрагента
	/// </summary>
	public class CounterpartyOverdueDebtorDebtAggregatedNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Id организации
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Суммарная просроченная дебиторская задолженность
		/// </summary>
		public decimal TotalOverdueDebtorDebt { get; set; }

		/// <summary>
		/// Номер договора
		/// </summary>
		public string ContractNumber { get; set; }
	}
}
