using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class BalanceSummaryRow
	{
		public int Num { get; set; }
		public int EntityId { get; set; }
		public string NomTitle { get; set; }
		public string InventoryNumber { get; set; }
		public decimal Min { get; set; }
		public decimal Common => WarehousesBalances.Sum() + EmployeesBalances.Sum() + CarsBalances.Sum();
		public decimal Diff => Common - Min;
		public decimal? ReservedItemsAmount { get; set; } = 0;
		public decimal? AvailableItemsAmount => Common - ReservedItemsAmount;
		public decimal PurchasePrice { get; set; }
		public decimal Price { get; set; }
		public decimal AlternativePrice { get; set; }
		public List<decimal> WarehousesBalances { get; set; }
		public List<decimal> EmployeesBalances { get; set; }
		public List<decimal> CarsBalances { get; set; }
	}
}
