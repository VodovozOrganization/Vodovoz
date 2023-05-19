using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;

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
			//yentryId.Binding
			//	.AddBinding(ViewModel, vm => vm.IdPart, w => w.Text)
			//	.InitializeFromSource();

			yenumExpenseDocumentType.ItemsEnum = typeof(ExpenseInvoiceDocumentType);
			yenumExpenseDocumentType.ShowSpecialStateAll = true;
			yenumExpenseDocumentType.Binding
				.AddBinding(ViewModel, e => e.ExpenseDocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumIncomeDocumentType.ItemsEnum = typeof(IncomeInvoiceDocumentType);
			yenumIncomeDocumentType.ShowSpecialStateAll = true;
			yenumIncomeDocumentType.Binding
				.AddBinding(ViewModel, e => e.IncomeDocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active)
				.InitializeFromSource();

			entryParentGroup.ViewModel = ViewModel.ParentGroupViewModel;

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;
		}
	}
}
