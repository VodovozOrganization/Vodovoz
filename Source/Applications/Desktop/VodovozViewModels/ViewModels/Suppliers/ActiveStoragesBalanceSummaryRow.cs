namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class ActiveStoragesBalanceSummaryRow
	{
		/// <summary>
		/// Порядковый номер хранилища
		/// </summary>
		public int? RowNumberStorage { get; set; }
		public int StorageId { get; set; }
		public string Storage { get; set; }
		/// <summary>
		/// Порядковый номер ТМЦ на балансе хранилища
		/// </summary>
		public int RowNumberFromStorage { get; set; }
		public int EntityId { get; set; }
		public string Entity { get; set; }
		public string InventoryNumber { get; set; }
		public decimal Balance { get; set; }
	}
}
