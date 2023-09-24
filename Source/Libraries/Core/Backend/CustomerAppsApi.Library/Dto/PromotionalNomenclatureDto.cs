namespace CustomerAppsApi.Library.Dto
{
	public class PromotionalNomenclatureDto
	{
		public int ErpNomenclatureId { get; set; }
		public int Count { get; set; }
		public decimal Discount { get; set; }
		public bool IsDiscountMoney { get; set; }
	}
}
