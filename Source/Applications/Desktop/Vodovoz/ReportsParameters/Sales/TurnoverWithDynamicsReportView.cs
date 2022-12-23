using QS.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.ViewModels.Reports.Sales;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class TurnoverWithDynamicsReportView : ViewBase<TurnoverWithDynamicsReportViewModel>
	{
		private SelectableParameterReportFilterView _filterView;
		private const string _radioButtonPrefix = "yrbtn";
		private const string _sliceRadioButtonGroupPrefix = "Slice";
		private const string _measurementUnitRadioButtonGroupPrefix = "MeasurementUnit";
		private const string _dynamicsInRadioButtonGroupPrefix = "DynamicsIn";
		private readonly Dictionary<string, string[]> _allowedRadioButtonsGroupsValues = new Dictionary<string, string[]>();

		public TurnoverWithDynamicsReportView(TurnoverWithDynamicsReportViewModel viewModel) : base(viewModel)
		{
			_allowedRadioButtonsGroupsValues.Add(
				_measurementUnitRadioButtonGroupPrefix,
				new[]
				{
					"Amount",
					"Price"
				});

			_allowedRadioButtonsGroupsValues.Add(
				_dynamicsInRadioButtonGroupPrefix,
				new[]
				{
					"Percents",
					"MeasurementUnit"
				});

			Build();
			ConfigureDlg();
			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ConfigureDlg()
		{
			btnReportInfo.Clicked += (s, e) => ViewModel.ShowInfoCommand.Execute();
			ViewModel.ShowInfoCommand.CanExecuteChanged += (s, e) => btnReportInfo.Sensitive = ViewModel.ShowInfoCommand.CanExecute();

			buttonCreateReport.Clicked += (s, e) => ViewModel.LoadReportCommand.Execute();
			ViewModel.LoadReportCommand.CanExecuteChanged += (s, e) => buttonCreateReport.Sensitive = ViewModel.LoadReportCommand.CanExecute();

			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			btnReportInfo.Clicked += (s, e) => ViewModel.ShowInfoCommand.Execute();

			foreach(Gtk.RadioButton radioButton in yrbtnSliceDay.Group)
			{
				if(radioButton.Active)
				{
					SliceGroupSelectionChanged(radioButton, EventArgs.Empty);
				}

				radioButton.Toggled += SliceGroupSelectionChanged;
			}

			foreach(Gtk.RadioButton radioButton in yrbtnMeasurementUnitAmount.Group)
			{
				if(radioButton.Active)
				{
					MeasurementUnitGroupSelectionChanged(radioButton, EventArgs.Empty);
				}

				radioButton.Toggled += MeasurementUnitGroupSelectionChanged;
			}

			foreach(Gtk.RadioButton radioButton in yrbtnDynamicsInPercents.Group)
			{
				if(radioButton.Active)
				{
					DynamicsInGroupSelectionChanged(radioButton, EventArgs.Empty);
				}

				radioButton.Toggled += DynamicsInGroupSelectionChanged;
			}

			ychkbtnShowDynamics.Binding
				.AddBinding(ViewModel, vm => vm.ShowDynamics, w => w.Active)
				.InitializeFromSource();
			ychkbtnShowLastSale.Binding
				.AddBinding(ViewModel, vm => vm.ShowLastSale, w => w.Active)
				.InitializeFromSource();

			ShowFilter();
		}

		private void DynamicsInGroupSelectionChanged(object s, EventArgs e)
		{
			if(s is Gtk.RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_dynamicsInRadioButtonGroupPrefix, string.Empty);

				if(_allowedRadioButtonsGroupsValues[_dynamicsInRadioButtonGroupPrefix]
					.Contains(trimmedName))
				{
					ViewModel.DynamicsIn = trimmedName;
				}
			}
		}

		private void MeasurementUnitGroupSelectionChanged(object s, EventArgs e)
		{
			if(s is Gtk.RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_measurementUnitRadioButtonGroupPrefix, string.Empty);

				if(_allowedRadioButtonsGroupsValues[_measurementUnitRadioButtonGroupPrefix]
					.Contains(trimmedName))
				{
					ViewModel.MeasurementUnit = trimmedName;
				}
			}
		}

		private void SliceGroupSelectionChanged(object s, EventArgs e)
		{
			if(s is Gtk.RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_sliceRadioButtonGroupPrefix, string.Empty);

				if(TurnoverWithDynamicsReportViewModel.SliceValues
					.Contains(trimmedName))
				{
					ViewModel.Slice = trimmedName;
				}
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.FilterViewModel):
					ShowFilter();
					break;
				default:
					break;
			}
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new SelectableParameterReportFilterView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.Show();
		}
	}
}
