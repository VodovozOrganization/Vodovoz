namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные по контрагенту и договору
	/// </summary>
	public class CounterpartyContractDataNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Id договора
		/// </summary>
		public int ContractId { get; set; }
	}
}
