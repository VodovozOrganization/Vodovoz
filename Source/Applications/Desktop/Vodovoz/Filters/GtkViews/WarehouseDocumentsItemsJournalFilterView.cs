using Gtk;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Documents;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;

namespace Vodovoz.Filters.GtkViews
{
	public partial class WarehouseDocumentsItemsJournalFilterView : FilterViewBase<WarehouseDocumentsItemsJournalFilterViewModel>
	{
		private const string _radioButtonPrefix = "yrbtn";
		private const string _targetSourcePrefix = "TargetSource";
		private SelectableParameterReportFilterView _filterView;

		public WarehouseDocumentsItemsJournalFilterView(WarehouseDocumentsItemsJournalFilterViewModel filterViewModel)
			: base(filterViewModel)
		{
			Build();

			dateperiodDocs.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			enumcomboDocumentType.ItemsEnum = typeof(DocumentType);
			enumcomboDocumentType.HiddenItems = new object[]
			{
				DocumentType.DeliveryDocument,
				DocumentType.DriverTerminalGiveout,
				DocumentType.DriverTerminalMovement,
				DocumentType.DriverTerminalReturn,
			};

			enumcomboDocumentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Driver, w => w.Subject)
				.InitializeFromSource();

			comboMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
			comboMovementStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowMovementDocumentFilterDetails, w => w.Visible)
				.AddBinding(vm => vm.MovementDocumentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ylabelMovementStatus.Binding
				.AddBinding(ViewModel, vm => vm.ShowMovementDocumentFilterDetails, w => w.Visible)
				.InitializeFromSource();

			foreach(RadioButton radioButton in yrbtnTargetSourceSource.Group)
			{
				if(radioButton.Active)
				{
					TargetSourceGroupSelectionChanged(radioButton, EventArgs.Empty);
				}

				radioButton.Toggled += TargetSourceGroupSelectionChanged;
			}

			ShowFilter();
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new SelectableParameterReportFilterView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.Show();
		}

		private void TargetSourceGroupSelectionChanged(object sender, EventArgs e)
		{
			if(sender is RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_targetSourcePrefix, string.Empty);

				ViewModel.TargetSource = (TargetSource)Enum.Parse(typeof(TargetSource), trimmedName);
			}
		}
	}
}
