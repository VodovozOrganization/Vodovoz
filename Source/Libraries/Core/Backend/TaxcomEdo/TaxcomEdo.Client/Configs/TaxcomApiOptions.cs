namespace TaxcomEdo.Client.Configs
{
	public class TaxcomApiOptions
	{
		public const string Path = "TaxcomApiOptions";
		
		public string BaseAddress { get; set; }
		public string SendUpdEndpoint { get; set; }
		public string SendBillEndpoint { get; set; }
		public string SendBillWithoutShipmentForDebtEndpoint { get; set; }
		public string SendBillWithoutShipmentForPaymentEndpoint { get; set; }
		public string SendBillWithoutShipmentForAdvancePaymentEndpoint { get; set; }
		public string GetContactListUpdatesEndPoint { get; set; }
		public string AcceptContactEndPoint { get; set; }
		public string GetDocFlowRawDataEndPoint { get; set; }
		public string GetDocFlowsUpdatesEndPoint { get; set; }
		public string AutoSendReceiveEndpoint { get; set; }
		public string OfferCancellationEndpoint { get; set; }
	}
}
