using System.Text.Json.Serialization;

namespace Vodovoz.Mindbox.Contracts
{
	/// <summary>
	/// Тело запроса на создание контрагента в Mindbox
	/// </summary>
	public sealed class MindboxCounterpartyRequest
	{
		[JsonPropertyName("ERPcounterpartyId")]
		public int ErpCounterpartyId { get; set; }

		[JsonPropertyName("CounterpartyForm")]
		public string CounterpartyForm { get; set; }

		[JsonPropertyName("CounterpartyType")]
		public string CounterpartyType { get; set; }

		[JsonPropertyName("IsNet")]
		public bool IsNet { get; set; }

		[JsonPropertyName("SourcePointOfContact")]
		public string SourcePointOfContact { get; set; }

		[JsonPropertyName("PhysicalFields")]
		public MindboxPhysicalFields PhysicalFields { get; set; }

		[JsonPropertyName("LegalFields")]
		public MindboxLegalFields LegalFields { get; set; }

		[JsonPropertyName("MobilePhone")]
		public string MobilePhone { get; set; }

		[JsonPropertyName("Email")]
		public string Email { get; set; }

		[JsonPropertyName("DeliveryPointFields")]
		public MindboxDeliveryPointFields DeliveryPointFields { get; set; }
	}
}
