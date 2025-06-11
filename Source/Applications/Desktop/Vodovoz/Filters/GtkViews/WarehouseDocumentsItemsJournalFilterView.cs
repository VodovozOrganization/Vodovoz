using Gtk;
using QS.Views.GtkUI;
using QS.Widgets;
using System;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewWidgets.Reports;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	public partial class WarehouseDocumentsItemsJournalFilterView : FilterViewBase<WarehouseDocumentsItemsJournalFilterViewModel>
	{
		private const string _radioButtonPrefix = "yrbtn";
		private const string _targetSourcePrefix = "TargetSource";
		private SelectableParameterReportFilterView _filterView;
		private IncludeExludeFiltersView _includeExcludefilterView;

		public WarehouseDocumentsItemsJournalFilterView(WarehouseDocumentsItemsJournalFilterViewModel filterViewModel)
			: base(filterViewModel)
		{
			Build();
			dppDocuments.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yecmbDocumentType.ItemsEnum = typeof(DocumentType);
			yecmbDocumentType.HiddenItems = new object[]
			{
				DocumentType.DeliveryDocument
			};

			entryDocumentId.ValidationMode = ValidationType.Numeric;
			entryDocumentId.KeyReleaseEvent += OnKeyReleased;
			entryDocumentId.Binding.AddBinding(ViewModel, vm => vm.DocumentId, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();

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

			btnInfo.Clicked += OnBInfoBtnClicked;
		}

		private void OnBInfoBtnClicked(object sender, EventArgs e)
		{
			ViewModel.ShowInfoCommand?.Execute();
		}

		private void OnKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new SelectableParameterReportFilterView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.Show();

			_includeExcludefilterView?.Destroy();
			_includeExcludefilterView = new IncludeExludeFiltersView(ViewModel.IncludeExcludeFilterViewModel);
			_includeExcludefilterView.HeightRequest = 200;
			yvboxParameters.Add(_includeExcludefilterView);
			_includeExcludefilterView.Show();
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
