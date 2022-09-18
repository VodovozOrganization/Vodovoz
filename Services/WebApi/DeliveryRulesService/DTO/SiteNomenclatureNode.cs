using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class SiteNomenclatureNode
	{
		[JsonInclude]
		public int SiteId { get; set; }

		[JsonInclude]
		public int? ERPId { get; set; }

		[JsonInclude]
		public int Amount { get; set; }
	}
}
