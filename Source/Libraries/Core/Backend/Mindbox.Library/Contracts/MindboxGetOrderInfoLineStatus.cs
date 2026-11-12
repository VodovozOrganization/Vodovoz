using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Статус позиции в ответе GetOrderInfo
	/// </summary>
	public sealed class MindboxGetOrderInfoLineStatus
	{
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
