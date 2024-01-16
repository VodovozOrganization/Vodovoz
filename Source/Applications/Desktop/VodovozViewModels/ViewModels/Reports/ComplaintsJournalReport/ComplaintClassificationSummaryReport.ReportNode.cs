namespace Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport
{
	public partial class ComplaintClassificationSummaryReport
	{
		public class ReportNode
		{
			public string Guilties { get; set; }
			public string ComplaintObject { get; set; }
			public string ComplaintKind { get; set; }
			public string ComplaintDetalization { get; set; }
			public int Amount { get; set; }
		}
	}
}
