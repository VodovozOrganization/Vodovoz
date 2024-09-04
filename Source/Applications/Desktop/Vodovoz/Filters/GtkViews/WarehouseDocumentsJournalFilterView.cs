using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Documents;
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
			yenumcomboboxDocumentType.HiddenItems = new[] { DocumentType.DeliveryDocument as object };
			yenumcomboboxDocumentType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Warehouse, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanChangeRestrictedDocumentType, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboboxMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
			yenumcomboboxMovementStatus.Binding
				.AddBinding(ViewModel, vm => vm.MovementDocumentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryWarehouse.ViewModel = ViewModel.WarehouseEntityEntryViewModel;

			entityentryDriver.ViewModel = ViewModel.DriverEntityEntryViewModel;
		}
	}
}
