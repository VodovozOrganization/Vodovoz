namespace EdoDocumentsConsumer.Configs
{
	public class TaxcomApiOptions
	{
		public const string Path = "TaxcomApiOptions";
		
		public string BaseAddress { get; set; }
		public string SendUpdEndpoint { get; set; }
		public string SendBillEndpoint { get; set; }
		public string SendBillsWithoutShipmentEndpoint { get; set; }
	}
}
