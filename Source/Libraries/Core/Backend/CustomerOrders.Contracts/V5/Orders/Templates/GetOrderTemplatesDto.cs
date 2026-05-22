namespace CustomerOrders.Contracts.V5.Orders.Templates
{
	public class GetOrderTemplatesDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int CounterpartyErpId { get; set; }
		/// <summary>
		/// Номер страницы
		/// </summary>
		public int Page { get; set; }
		/// <summary>
		/// Количество для отображения на странице
		/// </summary>
		public int TemplatesCountOnPage { get; set; }
	}
}
