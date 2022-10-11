using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class DeliveryRulesRequest
	{
		[JsonInclude]
		public decimal Latitude { get; set; }

		[JsonInclude]
		public decimal Longitude { get; set; }

		[JsonInclude]
		public SiteNomenclatureNode[] SiteNomenclatures { get; set; }
	}
}
