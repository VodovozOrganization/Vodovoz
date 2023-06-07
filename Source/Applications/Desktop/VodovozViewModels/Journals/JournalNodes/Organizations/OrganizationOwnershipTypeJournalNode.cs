using QS.Project.Journal;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Organizations
{
	public class OrganizationOwnershipTypeJournalNode : JournalEntityNodeBase<OrganizationOwnershipType>
	{
		public override string Title => FullName;
		public string Abbreviation { get; set; }
		public string FullName { get; set; }
		public bool IsArchive { get; set; }
	}
}
