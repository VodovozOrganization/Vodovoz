using QS.Project.Journal;

namespace Vodovoz.EntityRepositories.Payments
{
	public class UnallocatedBalancesJournalNode : JournalNodeBase
	{
		public int CounterpartyId { get; set; }
		public int OrganizationId { get; set; }
		public string CounterpartyName { get; set; }
		public string CounterpartyINN { get; set; }
		public string OrganizationName { get; set; }
		public decimal CounterpartyBalance { get; set; }
		public decimal CounterpartyDebt { get; set; }
	}
}
