namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Запрос истории заказов контрагента
	/// </summary>
	public class CounterpartyOrderHistoryRequestDto
	{
		/// <summary>
		/// Идентификатор клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
	}
}
