using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderRequest
	{
		[JsonPropertyName("customer")]
		public MindboxLoyaltyOrderCustomer Customer { get; set; }

		[JsonPropertyName("order")]
		public MindboxLoyaltyOrder Order { get; set; }
	}
}
