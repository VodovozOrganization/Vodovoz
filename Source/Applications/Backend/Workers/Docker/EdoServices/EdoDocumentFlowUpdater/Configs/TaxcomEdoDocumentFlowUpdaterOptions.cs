namespace EdoDocumentFlowUpdater.Configs
{
	public class TaxcomEdoDocumentFlowUpdaterOptions
	{
		public static string Path => "DocumentFlowUpdaterOptions";
		
		public int DelayBetweenDocumentFlowProcessingInSeconds { get; set; }
		public string EdoAccount { get; set; }
	}
}
