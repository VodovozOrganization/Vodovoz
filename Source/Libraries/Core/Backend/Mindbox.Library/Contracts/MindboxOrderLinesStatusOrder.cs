using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Заказ в запросе смены статуса позиций
	/// </summary>
	public sealed class MindboxOrderLinesStatusOrder
	{
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }

		[JsonPropertyName("lines")]
		public List<MindboxOrderLinesStatusLine> Lines { get; set; }
	}
}
