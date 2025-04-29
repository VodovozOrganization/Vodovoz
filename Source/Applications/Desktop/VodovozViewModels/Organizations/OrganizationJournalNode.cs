using QS.Project.Journal;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Organizations
{
	public class OrganizationJournalNode : JournalEntityNodeBase<Organization>
	{
		public override string Title => Name;
		public string Name { get; set; }
	}
}
