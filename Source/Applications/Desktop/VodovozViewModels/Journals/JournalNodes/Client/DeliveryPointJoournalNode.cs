using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class DeliveryPointJournalNode : JournalEntityNodeBase<DeliveryPoint>
	{
		public override string Title => CompiledAddress;
		public string CompiledAddress { get; set; }
		public string LogisticsArea { get; set; }
		public string Address1c { get; set; }
		public string Counterparty { get; set; }
		public bool IsActive { get; set; }
		public bool FoundInFias { get; set; }
		public bool FixedInFias { get; set; }
		public string IdString => Id.ToString();
	}
}
