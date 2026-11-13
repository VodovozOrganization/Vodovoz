using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Балльный счёт в запросе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartBalanceType
	{
		/// <summary>
		/// Идентификаторы балльного счёта
		/// </summary>
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
