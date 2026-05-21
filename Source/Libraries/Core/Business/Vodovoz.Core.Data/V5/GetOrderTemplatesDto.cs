namespace Vodovoz.Core.Data.V5
{
	public class GetOrderTemplatesDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public int Source { get; set; }
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
