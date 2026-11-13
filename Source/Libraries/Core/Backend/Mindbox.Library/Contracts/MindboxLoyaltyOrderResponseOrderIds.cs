using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderResponseOrderIds
	{
		[JsonPropertyName("mindboxId")]
		public long? MindboxId { get; set; }

		[JsonPropertyName("externalID")]
		public string ExternalId { get; set; }

		[JsonPropertyName("erptestOrderID")]
		public string ErpTestOrderId { get; set; }
	}
}
