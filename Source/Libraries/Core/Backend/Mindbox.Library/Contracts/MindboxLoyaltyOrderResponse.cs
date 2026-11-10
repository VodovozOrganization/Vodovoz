using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderResponse
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("order")]
		public MindboxLoyaltyOrderResponseOrder Order { get; set; }
	}
}
