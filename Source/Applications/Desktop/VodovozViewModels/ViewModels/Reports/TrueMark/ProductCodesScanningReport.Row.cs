namespace Vodovoz.ViewModels.ViewModels.Reports.TrueMark
{
	public partial class ProductCodesScanningReport
	{
		public class Row
		{
			public int RowNumber { get; set; }
			public string DriverFIO { get; set; }
			public int TotalCodesCount { get; set; }
			public int SuccessfullyScannedCodesCount { get; set; }
			public decimal SuccessfullyScannedCodesPercent => ((decimal)SuccessfullyScannedCodesCount / TotalCodesCount) * 100;
			public int UnscannedCodesCount { get; set; }
			public decimal UnscannedCodesPercent => ((decimal)UnscannedCodesCount / TotalCodesCount) * 100;
			public int SingleDuplicatedCodesCount { get; set; }
			public decimal SingleDuplicatedCodesPercent => ((decimal)SingleDuplicatedCodesCount / TotalCodesCount) * 100;
			public int MultiplyDuplicatedCodesCount { get; set; }
			public decimal MultiplyDuplicatedCodesPercent => ((decimal)MultiplyDuplicatedCodesCount / TotalCodesCount) * 100;
			public int InvalidCodesCount { get; set; }
			public decimal InvalidCodesPercent => ((decimal)InvalidCodesCount / TotalCodesCount) * 100;
		}
	}
}
