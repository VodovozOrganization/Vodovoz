using System.Text.Json.Serialization;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Позиция онлайн-заказа в снимке Mindbox
	/// </summary>
	public sealed class MindboxOnlineOrderStateLine
	{
		[JsonPropertyName("lineId")]
		public string LineId { get; set; }

		[JsonPropertyName("lineNumber")]
		public int LineNumber { get; set; }

		[JsonPropertyName("productId")]
		public string ProductId { get; set; }

		[JsonPropertyName("quantity")]
		public decimal Quantity { get; set; }

		[JsonPropertyName("basePricePerItem")]
		public decimal BasePricePerItem { get; set; }

		[JsonPropertyName("bonusesWrittenOff")]
		public int BonusesWrittenOff { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }
	}
}
