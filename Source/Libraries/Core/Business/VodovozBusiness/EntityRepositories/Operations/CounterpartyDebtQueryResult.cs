namespace Vodovoz.EntityRepositories.Operations
{
	public class CounterpartyDebtQueryResult
	{
		public decimal Charged { get; set; }
		public decimal Payed { get; set; }
		public decimal Deposit { get; set; }
		public decimal Debt => Charged - (Payed - Deposit);
		public decimal Balance => Payed - Deposit - Charged;
	}
}