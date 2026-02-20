using DateTimeHelpers;
using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewWidgets.Reports;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel.TurnoverWithDynamicsReport;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class TurnoverWithDynamicsReportView : TabViewBase<TurnoverWithDynamicsReportViewModel>
	{
		private IncludeExludeFiltersView _filterView;
		private const string _radioButtonPrefix = "yrbtn";
		private const string _measurementUnitRadioButtonGroupPrefix = "MeasurementUnit";
		private Task _generationTask;

		public TurnoverWithDynamicsReportView(TurnoverWithDynamicsReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
			UpdateSliderArrow();
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

			yenumSlice.ItemsEnum = typeof(DateTimeSliceType);
			yenumSlice.ShowSpecialStateAll = false;
			yenumSlice.Binding
				.AddBinding(ViewModel, vm => vm.SlicingType, w => w.SelectedItem)
				.InitializeFromSource();

			foreach(RadioButton radioButton in yrbtnMeasurementUnitAmount.Group)
			{
				if(radioButton.Active)
				{
					MeasurementUnitGroupSelectionChanged(radioButton, EventArgs.Empty);
				}

				radioButton.Toggled += MeasurementUnitGroupSelectionChanged;
			}

			yenumDynamicsMeasurementUnit.ItemsEnum = typeof(DynamicsInEnum);
			yenumDynamicsMeasurementUnit.ShowSpecialStateAll = false;
			yenumDynamicsMeasurementUnit.Binding
				.AddBinding(ViewModel, vm => vm.DynamicsIn, w => w.SelectedItem)
				.InitializeFromSource();

			ychkbtnShowDynamics.Binding
				.AddBinding(ViewModel, vm => vm.ShowDynamics, w => w.Active)
				.InitializeFromSource();
			
			ychkbtnShowLastSale.Binding
				.AddBinding(ViewModel, vm => vm.ShowLastSale, w => w.Active)
				.InitializeFromSource();
			
			ychkbtnShowResidueForNomenclaturesWithoutSales.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowResidueForNomenclaturesWithoutSales, w => w.Active)
				.AddBinding(vm => vm.CanShowResidueForNomenclaturesWithoutSales, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonShowContacts.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowContacts, w => w.Sensitive)
				.AddBinding(vm => vm.ShowContacts, w => w.Active)
				.InitializeFromSource();

			ShowFilter();

			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;
			orderdatefilterview3.Visible = false;

			ytreeReportIndicatorsRows.RowActivated += OnReportRowActivated;
			ViewModel.PropertyChanged += ViewModelPropertyChanged;
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			hpaned1.Position = 680;
		}

		private void OnButtonAbortCreateReportClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancelationTokenSource.Cancel();
		}

		private void MeasurementUnitGroupSelectionChanged(object s, EventArgs e)
		{
			if(s is RadioButton rbtn && rbtn.Active)
			{
				var trimmedName = rbtn.Name
					.Replace(_radioButtonPrefix, string.Empty)
					.Replace(_measurementUnitRadioButtonGroupPrefix, string.Empty);

				ViewModel.MeasurementUnit = (MeasurementUnitEnum)Enum.Parse(typeof(MeasurementUnitEnum), trimmedName);
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
			ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report.DisplayRows;
			ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
		}

		private void ConfigureTreeView()
		{
			var columnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<TurnoverWithDynamicsReportRow>.Create();

			columnsConfig.AddColumn("")
				.AddTextRenderer(row => row.IsSubheaderRow ? "<b>№</b>" : row.Index, useMarkup: true);

			var firstColumnTitle = string.Join(" | ", ViewModel.Report.GroupingBy.Select(x => x.GetEnumTitle()));

			columnsConfig.AddColumn(firstColumnTitle).AddTextRenderer(row =>
				(row.IsSubheaderRow || row.IsTotalsRow) ? $"<b>{row.Title}</b>" : row.Title, useMarkup: true)
				.WrapWidth(350)
				.WrapMode(Pango.WrapMode.Word);

			if(ViewModel.Report.ShowContacts)
			{
				columnsConfig.AddColumn("Телефоны").AddTextRenderer(row => row.Phones);
				columnsConfig.AddColumn("E-mail").AddTextRenderer(row => row.Emails);
			}

			if(ViewModel.Report.ShowDynamics)
			{
				var columnsCount = ViewModel.Report.Slices.Count * 2;
				for(var i = 0; i < columnsCount; i++)
				{
					if(i % 2 == 0)
					{
						var index = i;
						var sliceIndex = i / 2;
						columnsConfig.AddColumn(ViewModel.Report.Slices[sliceIndex].ToString())
							.HeaderAlignment(0.5f)
							.AddTextRenderer(row => row.IsSubheaderRow ? "" :
								row.DynamicColumns[index])
							.XAlign(1);
					}
					else
					{
						var index = i;
						columnsConfig.AddColumn(ViewModel.Report.DynamicsInStringShort)
							.HeaderAlignment(0.5f)
							.AddTextRenderer(row => row.IsSubheaderRow ? "" :
								row.DynamicColumns[index])
							.XAlign(1);
					}
				}
			}
			else
			{
				for(var i = 0; i < ViewModel.Report.Slices.Count; i++)
				{
					var index = i;
					columnsConfig.AddColumn(ViewModel.Report.Slices[index].ToString())
						.HeaderAlignment(0.5f)
						.AddTextRenderer(row => row.IsSubheaderRow ? "" :
							row.SliceColumnValues[index].ToString(ViewModel.Report.MeasurementUnitFormat))
						.XAlign(1);
				}
			}

			columnsConfig.AddColumn("Всего за период")
				.AddNumericRenderer(row => row.IsSubheaderRow ? "" :
					row.RowTotal.ToString(ViewModel.Report.MeasurementUnitFormat))
				.XAlign(1);

			if(ViewModel.Report.ShowLastSale)
			{
				columnsConfig.AddColumn("Дата последней продажи")
					.AddTextRenderer(row =>
						row.IsSubheaderRow
						? ""
						: row.IsTotalsRow && row.LastSaleDetails.LastSaleDate == DateTime.MinValue
							? ""
							: row.LastSaleDetails.LastSaleDate.ToString("dd.MM.yyyy"));
				columnsConfig.AddColumn("Кол-во дней с момента последней отгрузки")
					.AddTextRenderer(row => row.IsSubheaderRow ? "" :
						row.LastSaleDetails.DaysFromLastShipment.ToString("0"));

				if(ViewModel.Report.ShowResiduesAtCreatedAt)
				{
					columnsConfig
						.AddColumn($"Остатки по всем складам на {ViewModel.Report.CreatedAt:dd.MM.yyyy HH:mm}")
						.AddTextRenderer(row => row.IsSubheaderRow ? "" :
							row.LastSaleDetails.WarhouseResidue.ToString("0"));
				}
			}
			
			columnsConfig.AddColumn("");

			ytreeReportIndicatorsRows.ColumnsConfig = columnsConfig.Finish();
			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;
		}

		private void OnReportRowActivated(object o, RowActivatedArgs args)
		{
			var row = ytreeReportIndicatorsRows.GetSelectedObject<TurnoverWithDynamicsReportRow>();

			if(row == null)
			{
				return;
			}

			var data = row.SliceColumnValues;
			GetClipboard(Gdk.Selection.Clipboard).Text = string.Join("  \t", data);
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.HeightRequest = ViewModel.FilterViewModel.Filters.Count * 21 + 70;
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

					Gtk.Application.Invoke((s, eventArgs) => { ViewModel.Report = report; });
				}
				catch(OperationCanceledException)
				{
					Gtk.Application.Invoke((s, eventArgs) => {
						if(ViewModel.LastGenerationErrors.Any())
						{
							ViewModel.ShowWarning(string.Join("\n", ViewModel.LastGenerationErrors));
							ViewModel.LastGenerationErrors = Enumerable.Empty<string>();
						}
						else
						{
							ViewModel.ShowWarning("Формирование отчета было прервано");
						}
					});
				}
				catch(Exception ex)
				{
					Gtk.Application.Invoke((s, eventArgs) => { throw ex; });
				}
				finally
				{
					Gtk.Application.Invoke((s, eventArgs) => { ViewModel.IsGenerating = false; });
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
						Gtk.Application.Invoke((s, eventArgs) =>
						{
							ViewModel.IsSaving = false;
							ybuttonSave.Label = "Сохранить";
						});
					}
				});
			}

			filechooser.Destroy();
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			scrolledwindow2.Visible = !scrolledwindow2.Visible;

			hpaned1.PositionSet = false;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = scrolledwindow2.Visible ? ArrowType.Left : ArrowType.Right;
		}

		public override void Dispose()
		{
			ybuttonSave.Clicked -= OnYbuttonSaveClicked;
			ybuttonCreateReport.Clicked -= OnButtonCreateReportClicked;
			ybuttonAbortCreateReport.Clicked -= OnButtonAbortCreateReportClicked;

			foreach(RadioButton radioButton in yrbtnMeasurementUnitAmount.Group)
			{
				radioButton.Toggled -= MeasurementUnitGroupSelectionChanged;
			}

			ViewModel.PropertyChanged -= ViewModelPropertyChanged;
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			base.Dispose();
		}
	}
}
