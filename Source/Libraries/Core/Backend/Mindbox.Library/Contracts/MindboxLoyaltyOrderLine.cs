using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderLine
	{
		[JsonPropertyName("lineId")]
		public string LineId { get; set; }

		[JsonPropertyName("lineNumber")]
		public int LineNumber { get; set; }

		[JsonPropertyName("product")]
		public MindboxProduct Product { get; set; }

		[JsonPropertyName("quantity")]
		public decimal Quantity { get; set; }

		[JsonPropertyName("quantityType")]
		public string QuantityType { get; set; }

		[JsonPropertyName("basePricePerItem")]
		public decimal BasePricePerItem { get; set; }

		[JsonPropertyName("status")]
		public MindboxLoyaltyOrderLineStatus Status { get; set; }
	}
}
