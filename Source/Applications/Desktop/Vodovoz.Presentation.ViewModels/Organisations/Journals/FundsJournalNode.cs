using QS.Project.Journal;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations.Journals
{
	public class FundsJournalNode : JournalNodeBase
	{
		public override string Title => Name;
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public AccountFillType DefaultAccountFillType { get; set; }
	}
}
