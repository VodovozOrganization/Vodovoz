using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderLineStatusIds
	{
		[JsonPropertyName("externalId")]
		public string ExternalId { get; set; }
	}
}
