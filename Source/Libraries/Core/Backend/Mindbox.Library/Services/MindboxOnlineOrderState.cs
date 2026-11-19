using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Состояние онлайн-заказа, отправленное или готовое к отправке в Mindbox
	/// </summary>
	public sealed class MindboxOnlineOrderState
	{
		[JsonPropertyName("onlineOrderId")]
		public int OnlineOrderId { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("deliveryCost")]
		public decimal DeliveryCost { get; set; }

		[JsonPropertyName("totalPrice")]
		public decimal TotalPrice { get; set; }

		[JsonPropertyName("bonusesWrittenOff")]
		public int BonusesWrittenOff { get; set; }

		[JsonPropertyName("bonusesToEarn")]
		public int? BonusesToEarn { get; set; }

		[JsonPropertyName("lines")]
		public List<MindboxOnlineOrderStateLine> Lines { get; set; } = new List<MindboxOnlineOrderStateLine>();
	}
}
