using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class WarehouseDocumentsJournalFilterView : FilterViewBase<WarehouseDocumentsJournalFilterViewModel>
	{
		public WarehouseDocumentsJournalFilterView(WarehouseDocumentsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yenumcomboboxDocumentType.ItemsEnum = typeof(DocumentType);
			yenumcomboboxDocumentType.HiddenItems = ViewModel.DocumentTypesNotAllowedToSelect;
			yenumcomboboxDocumentType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentType, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanChangeRestrictedDocumentType, w => w.Sensitive)
				.InitializeFromSource();

			ychkbtnQRScanRequired.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowOnlyQRScanRequiredCarLoadDocuments, w => w.Visible)
				.AddBinding(vm => vm.OnlyQRScanRequiredCarLoadDocuments, w => w.Active)
				.InitializeFromSource();

			yenumcomboboxMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
			yenumcomboboxMovementStatus.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.MovementDocumentStatus, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanSelectMovementStatus, w => w.Visible)
				.InitializeFromSource();

			ylabelMovementStatus.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectMovementStatus, w => w.Visible)
				.InitializeFromSource();

			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryWarehouse.ViewModel = ViewModel.WarehouseEntityEntryViewModel;
			entityentryDriver.ViewModel = ViewModel.DriverEntityEntryViewModel;
			entityentryEmployee.ViewModel = ViewModel.EmployeeEntityEntryViewModel;
			entityentryCar.ViewModel = ViewModel.CarEntityEntryViewModel;
		}
	}
}
