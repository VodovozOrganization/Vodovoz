using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureInstanceBalanceNode
	{
		public int InstanceId { get; set; }
		public string InstanceName { get; set; }
		public string InventoryNumber { get; set; }
		public InventoryNomenclatureInstance InventoryNomenclatureInstance { get; set; }
		public decimal Balance { get; set; }
	}
}
