using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderResponseOrder
	{
		[JsonPropertyName("processingStatus")]
		public string ProcessingStatus { get; set; }

		[JsonPropertyName("ids")]
		public MindboxLoyaltyOrderResponseOrderIds Ids { get; set; }

		[JsonPropertyName("bonusPointsChanges")]
		public List<MindboxAuthorizedCartBonusPointsChange> BonusPointsChanges { get; set; }
	}
}
