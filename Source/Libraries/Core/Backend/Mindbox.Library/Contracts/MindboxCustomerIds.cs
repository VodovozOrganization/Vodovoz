using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Идентификаторы клиента Mindbox
	/// </summary>
	public sealed class MindboxCustomerIds
	{
		[JsonPropertyName("externalid")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string ExternalId { get; set; }

		[JsonPropertyName("erptestID")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string ErpTestId { get; set; }
	}
}
