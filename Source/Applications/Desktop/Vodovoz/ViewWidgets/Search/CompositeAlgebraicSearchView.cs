using Gtk;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.Widgets.Search;
using Key = Gdk.Key;

namespace Vodovoz.ViewWidgets.Search
{
	[ToolboxItem(true)]
	public partial class CompositeAlgebraicSearchView : WidgetViewBase<CompositeAlgebraicSearchViewModel>
	{
		public CompositeAlgebraicSearchView(CompositeAlgebraicSearchViewModel viewModel): base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			var hiddenOperandType = new object[] { OperandType.Disabled };

			ycmbeOperand1.ItemsEnum = typeof(OperandType);
			ycmbeOperand1.HiddenItems = hiddenOperandType;
			ycmbeOperand1.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Operand1, w => w.SelectedItem)
				.AddFuncBinding(vm => vm.Operand1 != OperandType.Disabled, w => w.Visible)
				.InitializeFromSource();

			ycmbeOperand2.ItemsEnum = typeof(OperandType);
			ycmbeOperand2.HiddenItems = hiddenOperandType;
			ycmbeOperand2.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Operand2, w => w.SelectedItem)
				.AddFuncBinding(vm => vm.Operand2 != OperandType.Disabled, w => w.Visible)
				.InitializeFromSource();

			ycmbeOperand3.ItemsEnum = typeof(OperandType);
			ycmbeOperand3.HiddenItems = hiddenOperandType;
			ycmbeOperand3.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Operand3, w => w.SelectedItem)
				.AddFuncBinding(vm => vm.Operand3 != OperandType.Disabled, w => w.Visible)
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

			buttonAdd.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.CanAddSearchEntry, w => w.Sensitive)
				.InitializeFromSource();

			buttonRemove.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.CanRemoveSearchEntry, w => w.Sensitive)
				.InitializeFromSource();

			buttonAdd.Clicked += (o, e) => ViewModel.AddSearchEntryCommand.Execute();
			buttonRemove.Clicked += (o, e) => ViewModel.RemoveSearchEntryCommand.Execute();
			buttonSearchClear.Clicked += (o, e) => ViewModel.ClearSearchEntriesTextCommand.Execute();
			buttonInformation.Clicked += (o, e) => ViewModel.ShowSearchInformation.Execute();

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
