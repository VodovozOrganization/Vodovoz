namespace CustomerOrders.Contracts.V5.Orders.Templates
{
	public class GetTemplateDiscountInfoDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int CounterpartyErpId { get; set; }
	}
}
