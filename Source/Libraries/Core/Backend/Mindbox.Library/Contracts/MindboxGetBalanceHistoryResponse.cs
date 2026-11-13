using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Ответ операции GetBalanceHistory
	/// </summary>
	public sealed class MindboxGetBalanceHistoryResponse
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("balances")]
		public List<MindboxBalanceHistoryItem> Balances { get; set; }
	}
}
