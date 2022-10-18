using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;

namespace Vodovoz.Filters.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UsersJournalFilterView : FilterViewBase<UsersJournalFilterViewModel>
	{
		public UsersJournalFilterView(UsersJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			chkShowDeactivatedUsers.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowDeactivatedUsers, w => w.Active)
				.InitializeFromSource();
		}
	}
}
