using QS.Project.Journal;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Complaints.ComplaintResults
{
	public class ComplaintResultsOfEmployeesJournalNode : JournalEntityNodeBase<ComplaintResultOfEmployees>
	{
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public override string Title => Name;
	}
}
