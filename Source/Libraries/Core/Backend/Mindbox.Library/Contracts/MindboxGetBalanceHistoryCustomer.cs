using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Клиент в запросе GetBalanceHistory
	/// </summary>
	public sealed class MindboxGetBalanceHistoryCustomer
	{
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
