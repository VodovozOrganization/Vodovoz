namespace Vodovoz.EntityRepositories.Operations
{
	public class DepositsQueryResult
	{
		public decimal Received { get; set; }
		public decimal Refund { get; set; }
		public decimal Deposits => Received - Refund;
	}
}
