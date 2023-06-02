using Gtk;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Key = Gdk.Key;

namespace Vodovoz.Cash.FinancialCategoriesGroups
{
	[ToolboxItem(true)]
	public partial class FinancialCategoriesJournalFilterView : FilterViewBase<FinancialCategoriesJournalFilterViewModel>
	{
		public FinancialCategoriesJournalFilterView(FinancialCategoriesJournalFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			yentrySearch.Binding
				.AddBinding(ViewModel, vm => vm.SearchString, w => w.Text)
				.InitializeFromSource();

			yentrySearch.KeyReleaseEvent += OnSearchKeyReleased;

			yenumTargetDocument.ItemsEnum = typeof(TargetDocument);
			yenumTargetDocument.ShowSpecialStateAll = true;
			yenumTargetDocument.Binding
				.AddBinding(ViewModel, e => e.TargetDocument, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active)
				.InitializeFromSource();

			entryParentGroup.ViewModel = ViewModel.ParentGroupViewModel;

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;
		}

		private void OnSearchKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}
