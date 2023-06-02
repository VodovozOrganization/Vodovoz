using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class BalanceSummaryReport
	{
		public DateTime EndDate { get; set; }
		public List<string> WarehouseStoragesTitles { get; set; }
		public List<string> EmployeeStoragesTitles { get; set; }
		public List<string> CarStoragesTitles { get; set; }
		public List<BalanceSummaryRow> SummaryRows { get; set; }
		
		public void RemoveWarehouseByIndex(int counter)
		{
			WarehouseStoragesTitles.RemoveAt(counter);
			SummaryRows.ForEach(row => row.WarehousesBalances.RemoveAt(counter));
		}
		
		public void RemoveEmployeeByIndex(int counter)
		{
			EmployeeStoragesTitles.RemoveAt(counter);
			SummaryRows.ForEach(row => row.EmployeesBalances.RemoveAt(counter));
		}
		
		public void RemoveCarByIndex(int counter)
		{
			CarStoragesTitles.RemoveAt(counter);
			SummaryRows.ForEach(row => row.CarsBalances.RemoveAt(counter));
		}
	}
}
