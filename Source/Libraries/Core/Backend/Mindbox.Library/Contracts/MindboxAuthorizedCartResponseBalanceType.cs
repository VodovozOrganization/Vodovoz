using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Балльный счёт в ответе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartResponseBalanceType
	{
		/// <summary>
		/// Идентификаторы балльного счёта
		/// </summary>
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
