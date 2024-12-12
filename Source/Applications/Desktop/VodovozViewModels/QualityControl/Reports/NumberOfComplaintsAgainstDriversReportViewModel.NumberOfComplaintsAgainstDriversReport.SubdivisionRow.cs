namespace Vodovoz.ViewModels.QualityControl.Reports
{
	public partial class NumberOfComplaintsAgainstDriversReportViewModel
	{
		public partial class NumberOfComplaintsAgainstDriversReport
		{
			public class SubdivisionRow
			{
				public string Subdivision { get; set; }
				public int ComplaintsCount { get; set; }
			}
		}
	}
}
