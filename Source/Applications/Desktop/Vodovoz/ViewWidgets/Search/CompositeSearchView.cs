using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.Widgets.Search;

namespace Vodovoz.ViewWidgets.Search
{
	public partial class CompositeSearchView : WidgetViewBase<CompositeSearchViewModel>
	{
		public CompositeSearchView(CompositeSearchViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ylabelSearchInfo.Binding
				.AddBinding(ViewModel, vm => vm.SearchInfoLabelText, w => w.Text)
				.InitializeFromSource();

			entrySearch1.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EntrySearchText1, w => w.Text)
				.InitializeFromSource();

			entrySearch2.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EntrySearchText2, w => w.Text)
				.AddFuncBinding(vm => vm.SearchEntryShownCount > 1, w => w.Visible)
				.InitializeFromSource();

			entrySearch3.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EntrySearchText3, w => w.Text)
				.AddFuncBinding(vm => vm.SearchEntryShownCount > 2, w => w.Visible)
				.InitializeFromSource();

			entrySearch4.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EntrySearchText4, w => w.Text)
				.AddFuncBinding(vm => vm.SearchEntryShownCount > 3, w => w.Visible)
				.InitializeFromSource();

			buttonAddAnd.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.CanAddSearchEntry, w => w.Sensitive)
				.InitializeFromSource();

			buttonAddAnd.Clicked += (o, e) => ViewModel.AddSearchEntryCommand.Execute();
			buttonSearchClear.Clicked += (o, e) => ViewModel.ClearSerarchEntriesTextCommand.Execute();
		}
	}
}
