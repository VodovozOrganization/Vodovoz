using QS.Project.Journal;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.JournalNodes
{
	public class OrganizationJournalNode : JournalEntityNodeBase<Organization>
	{
		public override string Title => Name;
		public string Name { get; set; }
	}
}
