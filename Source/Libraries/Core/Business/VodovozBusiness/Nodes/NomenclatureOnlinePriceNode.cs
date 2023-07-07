namespace Vodovoz.Nodes
{
	public class NomenclatureOnlinePriceNode
	{
		public int Id { get; set; }
		public int NomenclatureOnlineParametersId { get; set; }
		public int MinCount { get; set; }
		public decimal Price { get; set; }
		public decimal? PriceWithoutDiscount { get; set; }
	}
}