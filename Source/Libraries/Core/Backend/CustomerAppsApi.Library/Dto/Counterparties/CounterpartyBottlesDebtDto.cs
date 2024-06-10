namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Долг по 19л бутылям клиента
	/// </summary>
	public class CounterpartyBottlesDebtDto
	{
		/// <summary>
		/// Id клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Долг по 19л бутылям
		/// </summary>
		public int CounterpartyBottlesDebt { get; set; }
	}
}
