using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Идентификаторы клиента Mindbox
	/// </summary>
	public sealed class MindboxCustomerIds
	{
		[JsonPropertyName("erpId")]
		public string ErpId { get; set; }
	}
}
