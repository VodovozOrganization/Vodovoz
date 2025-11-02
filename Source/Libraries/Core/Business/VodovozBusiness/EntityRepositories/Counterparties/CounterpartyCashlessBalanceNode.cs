namespace Vodovoz.EntityRepositories.Counterparties
{
	public class CounterpartyCashlessBalanceNode
	{
		public int CounterpartyId { get; set; }
		public string CounterpartyInn { get; set; }
		public string CounterpartyName { get; set; }
		public decimal NotPaidOrdersSum { get; set; }
		public decimal PartiallyPaidOrdersSum { get; set; }
		public decimal CashlessMovementOperationsSum { get; set; }
		public decimal PaymentsFromBankClientSums { get; set; }
		public decimal UnallocatedBalance => CashlessMovementOperationsSum - PaymentsFromBankClientSums;
		public decimal Balance => (UnallocatedBalance + PartiallyPaidOrdersSum) - NotPaidOrdersSum;
	}
}

