using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Complaints
{
	public class ResponsibleJournalNode : JournalEntityNodeBase<Responsible>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool IsArchived { get; set; }
	}
}
