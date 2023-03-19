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
			dppDocuments.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			yecmbDocumentType.ItemsEnum = typeof(DocumentType);
			yecmbDocumentType.HiddenItems = new object[]
			{
				DocumentType.DeliveryDocument
			};

			yecmbDocumentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryDriver.ViewModel = ViewModel.DriverEntityEntryViewModel;
			entryDriver.Binding.AddBinding(ViewModel, vm => vm.CanReadEmployee, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryAuthor.ViewModel = ViewModel.AuthorEntityEntryViewModel;
			entryAuthor.Binding.AddBinding(ViewModel, vm => vm.CanReadEmployee, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryLastEditor.ViewModel = ViewModel.LastEditorEntityEntryViewModel;
			entryLastEditor.Binding.AddBinding(ViewModel, vm => vm.CanReadEmployee, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryNomenclature.ViewModel = ViewModel.NomenclatureEntityEntryViewModel;
			entryNomenclature.Binding.AddBinding(ViewModel, vm => vm.CanReadNomenclature, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			yecmbMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
			yecmbMovementStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowMovementDocumentFilterDetails, w => w.Visible)
				.AddBinding(vm => vm.MovementDocumentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ylblMovementStatus.Binding
				.AddBinding(ViewModel, vm => vm.ShowMovementDocumentFilterDetails, w => w.Visible)
				.InitializeFromSource();

			ychkbtnShowNotAffectedBalance.Binding
				.AddBinding(ViewModel, vm => vm.ShowNotAffectedBalance, w => w.Active)
				.InitializeFromSource();

			var initTargetSource = filterViewModel.TargetSource;
			foreach(RadioButton radioButton in yrbtnTargetSourceSource.Group)
			{
				radioButton.Active = radioButton.Name == _radioButtonPrefix + _targetSourcePrefix + initTargetSource.ToString();
				
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
