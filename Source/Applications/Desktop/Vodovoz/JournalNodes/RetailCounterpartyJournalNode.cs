using QS.Project.Journal;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.JournalNodes
{
	public class RetailCounterpartyJournalNode : JournalEntityNodeBase<Counterparty>
	{
		public override string Title => Name;

		public bool IsArhive { get; set; }
		
		public RevenueStatus? RevenueStatus { get; set; }

		public string Name { get; set; }

		public string INN { get; set; }

		public string Contracts { get; set; }

		public string Addresses { get; set; }

		public string Tags { get; set; }

		public string Phones { get; set; }

		public string PhonesDigits { get; set; }
	}
}
