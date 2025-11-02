using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public class InstanceOnStorageData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int? StorageId { get; set; }
		public string StorageName { get; set; }
		public decimal? Balance { get; set; }
		public string InventoryNumber { get; set; }
		public bool IsUsed { get; set; }
		public string GetInventoryNumber => InventoryNomenclatureInstance.GetInventoryNumberString(IsUsed, InventoryNumber);
	}
}
