using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	public sealed class MindboxLoyaltyOrderCustomer
	{
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
