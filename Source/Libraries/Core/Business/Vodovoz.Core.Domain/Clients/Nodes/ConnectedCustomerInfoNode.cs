namespace Vodovoz.Core.Domain.Clients.Nodes
{
	public class ConnectedCustomerInfoNode
	{
		public ConnectedCustomer ConnectedCustomer { get; set; }
		public string CounterpartyFullName { get; set; }
		
		public string BlockingReason
		{
			get => ConnectedCustomer.BlockingReason;
			set => ConnectedCustomer.BlockingReason = value;
		}

		public ConnectedCustomerConnectState ConnectState
		{
			get => ConnectedCustomer.ConnectState;
			set => ConnectedCustomer.ConnectState = value;
		}
	}
}
