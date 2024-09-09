using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PhonesJournalFilterView : FilterViewBase<PhonesJournalFilterViewModel>
	{
		public PhonesJournalFilterView(PhonesJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entityentryEmployee.ViewModel = ViewModel.EmployeeViewModel;
			HideAll();
		}
	}
}
