using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Ответ операции GetOrderInfo
	/// </summary>
	public sealed class MindboxGetOrderInfoResponse
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("order")]
		public MindboxGetOrderInfoOrder Order { get; set; }
	}
}
