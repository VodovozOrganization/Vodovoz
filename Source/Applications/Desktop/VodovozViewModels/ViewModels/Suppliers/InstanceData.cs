using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class InstanceData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal PurchasePrice { get; set; }
		public string InventoryNumber { get; set; }
		public bool IsUsed { get; set; }
		public string GetInventoryNumber => InventoryNomenclatureInstance.GetInventoryNumberString(IsUsed, InventoryNumber);
	}
}
