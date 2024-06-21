namespace Vodovoz.EntityRepositories.Operations
{
	class DepositsQueryResult
	{
		public decimal Received { get; set; }
		public decimal Refund { get; set; }
		public decimal Deposits => Received - Refund;
	}
}
