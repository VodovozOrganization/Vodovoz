namespace CustomerOrders.Contracts.V5.Orders.Templates
{
	/// <summary>
	/// Данные для получения информации о шаблоне автозаказа
	/// </summary>
	public class GetOrderTemplateInfoDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int OrderTemplateId { get; set; }
	}
}
