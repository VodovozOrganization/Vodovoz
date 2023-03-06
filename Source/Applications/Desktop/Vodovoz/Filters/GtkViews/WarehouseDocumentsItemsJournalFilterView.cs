using QS.Views.GtkUI;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WarehouseDocumentsItemsJournalFilterView : FilterViewBase<WarehouseDocumentsItemsJournalFilterViewModel>
	{
		public WarehouseDocumentsItemsJournalFilterView(WarehouseDocumentsItemsJournalFilterViewModel filterViewModel)
			: base(filterViewModel)
		{
			Build();

			dateperiodDocs.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeDatePeriod, w => w.Sensitive)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			enumcomboDocumentType.ItemsEnum = typeof(DocumentType);
			enumcomboDocumentType.HiddenItems = new[] { DocumentType.DeliveryDocument as object };

			enumcomboDocumentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeDocumentType, w => w.Sensitive)
				.AddBinding(vm => vm.DocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			evmeWarehouse.SetEntityAutocompleteSelectorFactory(ViewModel.WarehouseJournalFactory.CreateSelectorFactory());

			evmeWarehouse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeWarehouse, w => w.Sensitive)
				.AddBinding(vm => vm.CanUpdateWarehouse, w => w.CanEditReference)
				.AddBinding(vm => vm.Warehouse, w => w.Subject)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeDriver, w => w.Sensitive)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.InitializeFromSource();

			comboMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
			comboMovementStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowMovementDocumentFilterDetails, w => w.Visible)
				.AddBinding(vm => vm.CanChangeMovementDocumentStatus, w => w.Sensitive)
				.AddBinding(vm => vm.MovementDocumentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ylabelMovementStatus.Binding
				.AddBinding(ViewModel, vm => vm.ShowMovementDocumentFilterDetails, w => w.Visible)
				.InitializeFromSource();
		}
	}
}
