namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class ActiveStoragesBalanceSummaryRow
	{
		public int StorageId { get; set; }
		public string Storage { get; set; }
		public int RowNumberFromStorage { get; set; }
		public int EntityId { get; set; }
		public string Entity { get; set; }
		public string InventoryNumber { get; set; }
		public decimal Balance { get; set; }
	}
}
