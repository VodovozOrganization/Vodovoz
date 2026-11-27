using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Клиент, для которого рассчитывается список товаров
	/// </summary>
	public sealed class MindboxCalculateProductListCustomer
	{
		/// <summary>
		/// Идентификаторы клиента в Mindbox
		/// </summary>
		[JsonPropertyName("ids")]
		public Dictionary<string, string> Ids { get; set; }
	}
}
