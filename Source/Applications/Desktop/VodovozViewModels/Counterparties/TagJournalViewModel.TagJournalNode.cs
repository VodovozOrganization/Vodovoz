using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class TagJournalViewModel
	{
		public class TagJournalNode : JournalEntityNodeBase<Tag>
		{
			public override string Title => Name;

			public string Name { get; set; }
			public string ColorText { get; set; }
		}
	}
}
