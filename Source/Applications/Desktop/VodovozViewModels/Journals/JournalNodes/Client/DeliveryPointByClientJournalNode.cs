using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class DeliveryPointByClientJournalNode : JournalEntityNodeBase<DeliveryPoint>
	{
		public override string Title => CompiledAddress;
		public string CompiledAddress { get; set; }
		public bool IsActive { get; set; }
	}
}
