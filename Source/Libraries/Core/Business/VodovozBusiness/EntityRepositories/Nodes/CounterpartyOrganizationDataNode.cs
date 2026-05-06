namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные по контрагенту и организации
	/// </summary>
	public class CounterpartyOrganizationDataNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Id организации
		/// </summary>
		public int OrganizationId { get; set; }
	}
}
