using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Запрос операции GetOrderInfo
	/// </summary>
	public sealed class MindboxGetOrderInfoRequest
	{
		[JsonPropertyName("order")]
		public MindboxGetOrderInfoRequestOrder Order { get; set; }
	}
}
