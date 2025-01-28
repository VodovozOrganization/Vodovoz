using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.ChangingPaymentTypeByDriversReport
{
	public class ChangingPaymentTypeByDriversReport
	{
		public string SelectedGeoGroupName { get; set; }
		public List<ChangingPaymentTypeByDriversReportRow> Rows { get; set; } = new List<ChangingPaymentTypeByDriversReportRow>();
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
