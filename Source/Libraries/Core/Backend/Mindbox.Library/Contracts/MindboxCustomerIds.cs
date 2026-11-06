using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Идентификаторы клиента Mindbox
	/// </summary>
	public sealed class MindboxCustomerIds
	{
		[JsonPropertyName("externalid")]
		public string ExternalId { get; set; }
	}
}
