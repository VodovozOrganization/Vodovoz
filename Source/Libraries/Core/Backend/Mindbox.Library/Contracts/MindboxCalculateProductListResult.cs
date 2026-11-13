using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Результат расчёта списка товаров
	/// </summary>
	public sealed class MindboxCalculateProductListResult
	{
		/// <summary>
		/// Статус обработки списка товаров
		/// </summary>
		[JsonPropertyName("processingStatus")]
		public string ProcessingStatus { get; set; }

		/// <summary>
		/// Рассчитанные позиции списка товаров
		/// </summary>
		[JsonPropertyName("items")]
		public List<MindboxCalculateProductListResultItem> Items { get; set; }
	}
}
