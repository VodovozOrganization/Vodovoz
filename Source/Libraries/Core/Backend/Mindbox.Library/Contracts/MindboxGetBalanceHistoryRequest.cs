using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Запрос операции GetBalanceHistory
	/// </summary>
	public sealed class MindboxGetBalanceHistoryRequest
	{
		[JsonPropertyName("customer")]
		public MindboxGetBalanceHistoryCustomer Customer { get; set; }
	}
}
