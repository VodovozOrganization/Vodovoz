using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderLineStatus
	{
		[JsonPropertyName("ids")]
		public MindboxLoyaltyOrderLineStatusIds Ids { get; set; }
	}
}
