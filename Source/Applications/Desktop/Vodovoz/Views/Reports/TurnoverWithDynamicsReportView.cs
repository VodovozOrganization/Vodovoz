using DateTimeHelpers;
using Gtk;
using QS.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.ViewModels.Reports.Sales;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class TurnoverWithDynamicsReportView : ViewBase<TurnoverWithDynamicsReportViewModel>
	{
		private SelectableParameterReportFilterView _filterView;
		private const string _radioButtonPrefix = "yrbtn";
		private const string _sliceRadioButtonGroupPrefix = "Slice";
		private const string _measurementUnitRadioButtonGroupPrefix = "MeasurementUnit";
		private const string _dynamicsInRadioButtonGroupPrefix = "DynamicsIn";

		public TurnoverWithDynamicsReportView(TurnoverWithDynamicsReportViewModel viewModel) : base(viewModel)
		{
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

				ViewModel.DynamicsIn = (DynamicsInEnum)Enum.Parse(typeof(DynamicsInEnum), trimmedName);

			}
		}

		private void MeasurementUnitGroupSelectionChanged(object s, EventArgs e)
		{
			if(s is Gtk.RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_measurementUnitRadioButtonGroupPrefix, string.Empty);

				ViewModel.MeasurementUnit = (MeasurementUnitEnum)Enum.Parse(typeof(MeasurementUnitEnum), trimmedName);
			}
		}

		private void SliceGroupSelectionChanged(object s, EventArgs e)
		{
			if(s is Gtk.RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_sliceRadioButtonGroupPrefix, string.Empty);

				ViewModel.SlicingType = (DateTimeSliceType)Enum.Parse(typeof(DateTimeSliceType), trimmedName);
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.FilterViewModel):
					ShowFilter();
					break;
				case nameof(ViewModel.Report):
					ShowReport();
					break;
				default:
					break;
			}
		}

		private void ShowReport()
		{
			ConfigureTreeView();

			ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report.Rows;
			ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
		}

		private void ConfigureTreeView()
		{
			var columnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<IList<string>>.Create();

			for(int i = 0; i < ViewModel.Report.Columns.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn(ViewModel.Report.Columns[i])
					.HeaderAlignment(0.5f)
					.AddTextRenderer(row => row[0] == "Group" || row[0] == "№"
						? index == 0 && row[0] == "Group" ? "" : $"<b>{row[index]}</b>" // Жирные заголовки и пропуск служебного значения
						: row[index],
						useMarkup: true)
					.XAlign(1);
			}

			columnsConfig.AddColumn("");

			ytreeReportIndicatorsRows.ColumnsConfig = columnsConfig.Finish();

			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;
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
