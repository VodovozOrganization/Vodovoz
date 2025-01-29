using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.ChangingPaymentTypeByDriversReport
{
	[Appellative(Nominative = "Отчёт по изменению формы оплаты водителями")]
	public class ChangingPaymentTypeByDriversReport : IClosedXmlReport
	{
		public string SelectedGeoGroupName { get; set; }
		public List<ChangingPaymentTypeByDriversReportRow> Rows { get; set; } = new List<ChangingPaymentTypeByDriversReportRow>();
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public string TemplatePath => @".\Reports\Logistic\ChangingPaymentTypeByDriversReport.xlsx";
	}
}
