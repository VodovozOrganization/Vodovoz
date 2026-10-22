using System.Text.Json.Serialization;

namespace Vodovoz.Mindbox.Contracts
{
	/// <summary>
	/// Данные точки доставки
	/// </summary>
	public sealed class MindboxDeliveryPointFields
	{
		[JsonPropertyName("localityType")]
		public string LocalityType { get; set; }

		[JsonPropertyName("localityTypeShort")]
		public string LocalityTypeShort { get; set; }

		[JsonPropertyName("city")]
		public string City { get; set; }

		[JsonPropertyName("streetType")]
		public string StreetType { get; set; }

		[JsonPropertyName("streetTypeShort")]
		public string StreetTypeShort { get; set; }

		[JsonPropertyName("street")]
		public string Street { get; set; }

		[JsonPropertyName("building")]
		public string Building { get; set; }

		[JsonPropertyName("floor")]
		public string Floor { get; set; }

		[JsonPropertyName("entrance")]
		public string Entrance { get; set; }

		[JsonPropertyName("room")]
		public string Room { get; set; }

		[JsonPropertyName("latitude")]
		public double? Latitude { get; set; }

		[JsonPropertyName("longitude")]
		public double? Longitude { get; set; }

		[JsonPropertyName("deliveryPointCategoryId")]
		public int DeliveryPointCategoryId { get; set; }

		[JsonPropertyName("onlineComment")]
		public string OnlineComment { get; set; }

		[JsonPropertyName("intercom")]
		public string Intercom { get; set; }

		[JsonPropertyName("cityFiasGuid")]
		public string CityFiasGuid { get; set; }

		[JsonPropertyName("streetFiasGuid")]
		public string StreetFiasGuid { get; set; }

		[JsonPropertyName("buildingFiasGuid")]
		public string BuildingFiasGuid { get; set; }

		[JsonPropertyName("counterpartyErpId")]
		public int CounterpartyErpId { get; set; }

		[JsonPropertyName("source")]
		public int Source { get; set; }
	}
}
