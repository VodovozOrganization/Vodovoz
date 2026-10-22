using System.Text.Json.Serialization;

namespace Vodovoz.Mindbox.Contracts
{
	/// <summary>
	/// Данные юридического лица
	/// </summary>
	public sealed class MindboxLegalFields
	{
		[JsonPropertyName("OwnershipForm")]
		public string OwnershipForm { get; set; }

		[JsonPropertyName("CounterpartyName")]
		public string CounterpartyName { get; set; }

		[JsonPropertyName("CounterpartyFullName")]
		public string CounterpartyFullName { get; set; }

		[JsonPropertyName("PaymentForm")]
		public string PaymentForm { get; set; }

		[JsonPropertyName("INN")]
		public string INN { get; set; }

		[JsonPropertyName("KPP")]
		public string KPP { get; set; }
	}
}
