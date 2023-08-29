namespace Vodovoz.ViewModels.QualityControl.Reports
{
	public partial class NumberOfComplaintsAgainstDriversReportViewModel
	{
		public partial class NumberOfComplaintsAgainstDriversReport
		{
			public class Row
			{
				public string DriverFullName { get; set; }

				public int ComplaintsCount { get; set; }

				public string ComplaintsList { get; set; }
			}
		}
	}
}
