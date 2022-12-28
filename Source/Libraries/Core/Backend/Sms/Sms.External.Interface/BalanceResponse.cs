namespace Sms.External.Interface
{
	public class BalanceResponse : IBalanceResponse
	{
		public BalanceResponseStatus Status { get; set; }

		public BalanceType BalanceType { get; set; }

		public decimal BalanceValue { get; set; }
	}
}
