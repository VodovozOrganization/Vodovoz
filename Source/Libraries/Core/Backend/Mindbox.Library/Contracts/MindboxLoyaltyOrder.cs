using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrder
	{
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }

		[JsonPropertyName("deliveryCost")]
		public decimal DeliveryCost { get; set; }

		[JsonPropertyName("totalPrice")]
		public decimal TotalPrice { get; set; }

		[JsonPropertyName("bonusPoints")]
		public List<MindboxAuthorizedCartBonusPoint> BonusPoints { get; set; }

		[JsonPropertyName("lines")]
		public List<MindboxLoyaltyOrderLine> Lines { get; set; }
	}
}
