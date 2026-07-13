namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Промонабор
	/// </summary>
	public class PromotionalSetDto
	{
		/// <summary>
		/// Id промонабора в ДВ
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Наименование для ИПЗ
		/// </summary>
		public string OnlineName { get; set; }
		/// <summary>
		/// Для новых клиентов
		/// </summary>
		public bool ForNewClients { get; set; }
	}
}
