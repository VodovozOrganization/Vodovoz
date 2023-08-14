using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Cash.DocumentsJournal;

namespace Vodovoz.Cash.DocumentsJournal
{
	[ToolboxItem(true)]
	public partial class DocumentsFilterView : FilterViewBase<DocumentsFilterViewModel>
	{
		public DocumentsFilterView(DocumentsFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			enumcomboDocumentType.ItemsEnum = typeof(CashDocumentType);

			enumcomboDocumentType.Binding
				.AddBinding(ViewModel, vm => vm.CashDocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			dateperiodDocs.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entryEmployee.ViewModel = ViewModel.EmployeeViewModel;

			entryIncomeFinancialCategory.ViewModel = ViewModel.FinancialIncomeCategoryViewModel;

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			accessfilteredsubdivisionselectorwidget.Configure(ViewModel.UoW, true, ViewModel.DomainObjectsTypes);

			ViewModel.AvailableSubdivisions = accessfilteredsubdivisionselectorwidget.AvailableSubdivisions;

			accessfilteredsubdivisionselectorwidget.OnSelected += (s, e) =>
			{
				ViewModel.Subdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
			};
		}
	}
}
