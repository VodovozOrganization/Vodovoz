using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Позиция в запросе смены статуса
	/// </summary>
	public sealed class MindboxOrderLinesStatusLine
	{
		[JsonPropertyName("lineId")]
		public string LineId { get; set; }

		[JsonPropertyName("quantity")]
		public decimal Quantity { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }
	}
}
