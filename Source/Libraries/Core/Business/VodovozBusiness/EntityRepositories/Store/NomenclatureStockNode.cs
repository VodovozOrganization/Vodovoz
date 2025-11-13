namespace Vodovoz.EntityRepositories.Store
{
	public class NomenclatureStockNode
	{
		public int NomenclatureId { get; set; }
		public decimal Stock { get; set; }

		public NomenclatureStockNode()
		{

		}

		public NomenclatureStockNode(int nomenclatureId, decimal stock)
		{
			NomenclatureId = nomenclatureId;
			Stock = stock;
		}
	}
}
