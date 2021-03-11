using QS.Views.GtkUI;
using Vodovoz.Journals.FilterViewModels.Employees;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserJournalFilterView : FilterViewBase<UserJournalFilterViewModel>
	{
		public UserJournalFilterView(UserJournalFilterViewModel userJournalFilterViewModel) : base(userJournalFilterViewModel)
		{
			Build();
		}
	}
}
