using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.AverageFlowDiscrepanciesReport
{
	public class AverageFlowDiscrepanciesReport
	{
		public string SelectedCars { get; set; }
		public int SelectedDiscrepancyPercent { get; set; }
		public List<AverageFlowDiscrepanciesReportRow> Rows { get; set; } = new List<AverageFlowDiscrepanciesReportRow>();
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
