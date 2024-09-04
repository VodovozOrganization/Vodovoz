namespace TaxcomEdo.Library.Options
{
	public class DocumentFlowOptions
	{
		public const string Path = nameof(DocumentFlowOptions); 
		
		public int AddMonthForUpdPreparing { get; set; }
		public int AddDaysForBillsPreparing { get; set; }
		public int DelayBetweenPreparingSeconds { get; set; }
	}
}
