using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Товар в запросе и ответе Mindbox
	/// </summary>
	public sealed class MindboxProduct
	{
		/// <summary>
		/// Идентификаторы товара в Mindbox
		/// </summary>
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
