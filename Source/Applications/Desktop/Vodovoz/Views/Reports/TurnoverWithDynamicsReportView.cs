using DateTimeHelpers;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.ViewModels.Reports.Sales;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel.TurnoverWithDynamicsReport;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel.TurnoverWithDynamicsReport.TurnoverWithDynamicsReportRow;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class TurnoverWithDynamicsReportView : TabViewBase<TurnoverWithDynamicsReportViewModel>
	{
		private SelectableParameterReportFilterView _filterView;
		private const string _radioButtonPrefix = "yrbtn";
		private const string _sliceRadioButtonGroupPrefix = "Slice";
		private const string _measurementUnitRadioButtonGroupPrefix = "MeasurementUnit";
		private const string _dynamicsInRadioButtonGroupPrefix = "DynamicsIn";
		private Task _generationTask;

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

			ybuttonSave.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Clicked += OnYbuttonSaveClicked;

			ybuttonCreateReport.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsGenerating, w => w.Visible)
				.AddBinding(vm => vm.CanGenerate, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += OnButtonCreateReportClicked;

			ybuttonAbortCreateReport.Clicked += OnButtonAbortCreateReportClicked;

			ybuttonAbortCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsGenerating, w => w.Visible)
				.AddBinding(vm => vm.CanCancelGenerate, w => w.Sensitive)
				.InitializeFromSource();

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

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void OnButtonAbortCreateReportClicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
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
			var columnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<TurnoverWithDynamicsReportRow>.Create();

			columnsConfig.AddColumn("Периоды продаж").AddTextRenderer(row =>
				row.RowType == RowTypes.Totals ? $"<b>{row.Title}</b>" : row.Title, useMarkup: true).XAlign(1);

			for(var i = 0; i < ViewModel.Report.Slices.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn(ViewModel.Report.Slices[index].ToString())
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.SliceColumnValues[index])
					.XAlign(1);
			}

			columnsConfig.AddColumn("Всего за период").AddNumericRenderer(row => row.RowTotal);

			if(ViewModel.Report.ShowLastSale)
			{
				columnsConfig.AddColumn("Дата последней продажи").AddTextRenderer(row => row.LastSaleDetails.LastSaleDate.ToString("dd.MM.yyyy"));
				columnsConfig.AddColumn("Дней с последней продажи").AddTextRenderer(row => row.LastSaleDetails.DaysFromLastShipment.ToString("0"));
				columnsConfig.AddColumn($"Остатки на складе на {ViewModel.Report.CreatedAt:dd.MM.yyyy HH:mm:ss}").AddTextRenderer(row => row.LastSaleDetails.WarhouseResidue.ToString("0.000"));
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

		protected async void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancelationTokenSource = new CancellationTokenSource();

			ViewModel.IsGenerating = true;

			_generationTask = Task.Run(async () =>
			{
				try
				{
					var report = await ViewModel.ActionGenerateReport(ViewModel.ReportGenerationCancelationTokenSource.Token);

					Application.Invoke((s, eventArgs) => { ViewModel.Report = report; });
				}
				catch(OperationCanceledException)
				{
					Application.Invoke((s, eventArgs) => { ViewModel.ShowWarning("Формирование отчета было прервано"); });
				}
				catch(Exception ex)
				{
					Application.Invoke((s, eventArgs) => { throw ex; });
				}
				finally
				{
					Application.Invoke((s, eventArgs) => { ViewModel.IsGenerating = false; });
				}
			}, ViewModel.ReportGenerationCancelationTokenSource.Token);

			await _generationTask;
		}

		protected async void OnYbuttonSaveClicked(object sender, EventArgs e)
		{
			var extension = ".xlsx";

			var filechooser = new FileChooserDialog("Сохранить отчет...",
				null,
				FileChooserAction.Save,
				"Отменить", ResponseType.Cancel,
				"Сохранить", ResponseType.Accept)
			{
				DoOverwriteConfirmation = true,
				CurrentName = $"{Tab.TabName} {ViewModel.Report.CreatedAt:yyyy-MM-dd-HH-mm}{extension}"
			};

			var excelFilter = new FileFilter
			{
				Name = $"Документ Microsoft Excel ({extension})"
			};

			excelFilter.AddMimeType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			excelFilter.AddPattern($"*{extension}");
			filechooser.AddFilter(excelFilter);

			if(filechooser.Run() == (int)ResponseType.Accept)
			{
				var path = filechooser.Filename;

				if(!path.Contains(extension))
				{
					path += extension;
				}

				filechooser.Hide();

				ViewModel.IsSaving = true;

				await Task.Run(() =>
				{
					try
					{
						ybuttonSave.Label = "Отчет сохраняется...";
						ViewModel.ExportReport(path);
					}
					finally
					{
						Application.Invoke((s, eventArgs) =>
						{
							ViewModel.IsSaving = false;
							ybuttonSave.Label = "Сохранить";
						});
					}
				});
			}

			filechooser.Destroy();
		}
	}
}
