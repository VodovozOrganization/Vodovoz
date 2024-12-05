using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public class OrderChangesReport
	{
		private OrderChangesReport()
		{

		}

		public IList<OrderChangesReportRow> Rows { get; set; } = new List<OrderChangesReportRow>
		{
			new OrderChangesReportRow
			{
				RowNumber = 1,
				Counterparty = "Клиент",
				DriverPhoneComment = "Comment",
				PaymentDate = DateTime.Today.AddDays(-2),
				OrderId = 12345,
				OrderSum = 34.5m,
				DeliveryDate = DateTime.Today.AddDays(-1),
				ChangeTime = DateTime.Today,
				Nomenclature = "Water",
				OldValue = "OldValue",
				NewValue = "NewValue",
				Driver = "Driver",
				Author = "Author"
			}
		};

		public void ExportToExcel(string path)
		{

		}

		public static async Task<OrderChangesReport> Create()
		{
			return new OrderChangesReport();
		}
	}
}
