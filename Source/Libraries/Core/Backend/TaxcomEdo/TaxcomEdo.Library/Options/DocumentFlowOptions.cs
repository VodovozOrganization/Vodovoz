namespace TaxcomEdo.Library.Options
{
	public sealed class DocumentFlowOptions
	{
		public const string Path = nameof(DocumentFlowOptions); 
		
		public int AddMonthForUpdPreparing { get; set; }
		public int AddDaysForBillsPreparing { get; set; }
		public int DelayBetweenPreparingInSeconds { get; set; }
	}
}
