using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Клиент в запросе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartCustomer
	{
		/// <summary>
		/// Идентификаторы клиента
		/// </summary>
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
