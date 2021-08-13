using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport
{
	public class NomenclaturePlanReportMain
	{
		public IEnumerable<NomenclaturePlanReportRow> Rows { get; set; }
		public List<string> Titles { get; set; }
		public DateTime? FilterStartDate, FilterEndDate;
		public DateTime CreationDate { get; set; }
		public string ReportDates => FilterStartDate == FilterEndDate ? $"{FilterStartDate.Value.ToShortDateString()}"
			: $"период {FilterStartDate.Value.ToShortDateString()} - {FilterEndDate.Value.ToShortDateString()}";
	}
}