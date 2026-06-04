namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные по просроченным заказам с долгом
	/// </summary>
	public class OrderWithDebtNode
	{
		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Идентификатолр контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор организации
		/// </summary>
		public int OrganizationId { get; set; }

		/// <summary>
		/// Долг по заказу
		/// </summary>
		public decimal Debt { get; set; }
	}
}
