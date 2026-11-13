using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Позиция заказа в ответе GetOrderInfo
	/// </summary>
	public sealed class MindboxGetOrderInfoLine
	{
		[JsonPropertyName("lineId")]
		public string LineId { get; set; }

		[JsonPropertyName("quantity")]
		public decimal Quantity { get; set; }

		[JsonPropertyName("status")]
		public MindboxGetOrderInfoLineStatus Status { get; set; }

		[JsonPropertyName("appliedPromotions")]
		public List<MindboxAppliedPromotion> AppliedPromotions { get; set; }
	}
}
