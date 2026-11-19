using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Запрос операции изменения статуса позиций заказа
	/// </summary>
	public sealed class MindboxOrderLinesStatusRequest
	{
		[JsonPropertyName("executionDateTimeUtc")]
		public System.DateTime ExecutionDateTimeUtc { get; set; }

		[JsonPropertyName("orderLinesStatus")]
		public string OrderLinesStatus { get; set; }

		[JsonPropertyName("order")]
		public MindboxOrderLinesStatusOrder Order { get; set; }
	}
}
