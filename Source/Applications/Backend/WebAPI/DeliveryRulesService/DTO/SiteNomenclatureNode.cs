using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class SiteNomenclatureNode
	{
		[JsonPropertyOrder(0)]
		public int SiteId { get; set; }

		[JsonPropertyOrder(1)]
		public int? ERPId { get; set; }

		[JsonPropertyOrder(2)]
		public int Amount { get; set; }
	}
}
