using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class DeliveryRulesRequest
	{
		[JsonPropertyOrder(0)]
		public decimal Latitude { get; set; }

		[JsonPropertyOrder(1)]
		public decimal Longitude { get; set; }

		[JsonPropertyOrder(2)]
		public SiteNomenclatureNode[] SiteNomenclatures { get; set; }
	}
}
