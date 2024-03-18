using QS.Project.Journal;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public partial class RouteColumnJournalViewModel
	{
		public class RouteColumnJournalNode : JournalEntityNodeBase<RouteColumn>
		{
			public string Name { get; set; }
			public string ShortName { get; set; }
			public bool IsHighlighted { get; set; }

			public override string Title => Name;
		}
	}
}
