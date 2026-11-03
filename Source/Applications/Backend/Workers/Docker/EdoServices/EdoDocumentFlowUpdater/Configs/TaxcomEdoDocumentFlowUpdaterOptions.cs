namespace EdoDocumentFlowUpdater.Configs
{
	public class TaxcomEdoDocumentFlowUpdaterOptions
	{
		public static string Path => "DocumentFlowUpdaterOptions";
		
		public int DelayBetweenDocumentFlowProcessingInSeconds { get; set; }
		public int MaxMinutesInSentOrInProgress { get; set; } = 10;
		public string EdoAccount { get; set; }
	}
}
