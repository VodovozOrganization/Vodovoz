using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseCategoryJournalFilterView : FilterViewBase<ExpenseCategoryJournalFilterViewModel>
	{
		public ExpenseCategoryJournalFilterView(ExpenseCategoryJournalFilterViewModel journalFilterViewModel) : base(journalFilterViewModel)
		{
			this.Build();
			this.Configure();
		}

		private void Configure()
		{
			ycheckArchived.Binding.AddBinding(ViewModel, e => e.ShowArchive, w => w.Active).InitializeFromSource();
			yLevelenumcombobox1.ItemsEnum = typeof(LevelsFilter);
			yLevelenumcombobox1.Binding.AddBinding(ViewModel, e => e.Level, w => w.SelectedItem).InitializeFromSource();
			ycheckNotLinked.Binding.AddBinding(
					ViewModel,
					vm => vm.OnlyWithoutNewCategoryLink, w => w.Active)
				.InitializeFromSource();
		}
	}
}
