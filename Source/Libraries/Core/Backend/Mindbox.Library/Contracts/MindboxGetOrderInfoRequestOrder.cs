using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Заказ в запросе GetOrderInfo
	/// </summary>
	public sealed class MindboxGetOrderInfoRequestOrder
	{
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
