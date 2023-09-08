using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets.Search;
using Key = Gdk.Key;

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
			buttonSearchClear.Clicked += (o, e) => ViewModel.ClearSearchEntriesTextCommand.Execute();

			entrySearch1.KeyReleaseEvent += OnKeyReleased;
			entrySearch2.KeyReleaseEvent += OnKeyReleased;
			entrySearch3.KeyReleaseEvent += OnKeyReleased;
			entrySearch4.KeyReleaseEvent += OnKeyReleased;
		}

		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}
