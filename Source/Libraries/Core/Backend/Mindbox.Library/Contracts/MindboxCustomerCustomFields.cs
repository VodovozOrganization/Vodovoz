using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Дополнительные поля клиента Mindbox
	/// </summary>
	public sealed class MindboxCustomerCustomFields
	{
		[JsonPropertyName("CounterpartyForm")]
		public string CounterpartyForm { get; set; }
	}
}
