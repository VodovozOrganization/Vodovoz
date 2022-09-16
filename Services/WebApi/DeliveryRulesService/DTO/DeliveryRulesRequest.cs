
namespace DeliveryRulesService.DTO
{
	public class DeliveryRulesRequest
	{
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public SiteNomenclatureNode[] SiteNomenclatures { get; set; }
	}

	public class SiteNomenclatureNode
	{
		public int SiteId { get; set; }
		public int? ERPId { get; set; }
		public int Amount { get; set; }
	}
}
