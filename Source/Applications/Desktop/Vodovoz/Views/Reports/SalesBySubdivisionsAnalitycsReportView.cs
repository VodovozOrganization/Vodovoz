using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;

namespace Vodovoz.Views.Reports
{
	public partial class SalesBySubdivisionsAnalitycsReportView : TabViewBase<SalesBySubdivisionsAnalitycsReportViewModel>
	{
		private Task _generationTask;
		private SelectableParameterReportFilterView _filterView;

		public SalesBySubdivisionsAnalitycsReportView(SalesBySubdivisionsAnalitycsReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureReportView();
			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxTurnoverWithDynamicsReportFilterContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}

		private void ConfigureReportView()
		{
			dateFirstPeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.FirstPeriodStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.FirstPeriodEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			dateSecondPeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SecondPeriodStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.SecondPeriodEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ychkbtnSplitByNomenclatures.Binding
				.AddBinding(ViewModel, vm => vm.SplitByNomenclatures, w => w.Active)
				.InitializeFromSource();

			ychkbtnSplitBySubdivisions.Binding
				.AddBinding(ViewModel, vm => vm.SplitBySubdivisions, w => w.Active)
				.InitializeFromSource();

			ychkbtnSplitByWarhouses.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SplitByWarehouses, w => w.Active)
				.AddBinding(vm => vm.CanSplitByWarehouse, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanGenerate, w => w.Visible)
				.AddBinding(vm => vm.GenerateSensitive, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += OnButtonCreateReportClicked;

			ybuttonAbortCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanCancelGenerate, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Clicked += OnButtonAbortCreateReportClicked;

			ybuttonSave.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Clicked += OnYbuttonSaveClicked;
			ViewModel.PropertyChanged += ViewModelPropertyChanged;
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
			btnReportInfo.Clicked += (s, e) => ViewModel.ShowInfo();

			ShowFilter();
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxTurnoverWithDynamicsReportFilterContainer.Visible = !vboxTurnoverWithDynamicsReportFilterContainer.Visible;
			UpdateSliderArrow();
		}

		protected async void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancelationTokenSource = new CancellationTokenSource();

			ViewModel.IsGenerating = true;

			_generationTask = Task.Run(async () =>
			{
				try
				{
					var report = await ViewModel.GenerateReport(ViewModel.ReportGenerationCancelationTokenSource.Token);

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

		private void OnButtonAbortCreateReportClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancelationTokenSource.Cancel();
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
				CurrentName = ViewModel.Report?.Match(
					report => $"{report.Title} {report.CreatedAt:yyyy-MM-dd-HH-mm}{extension}",
					reportWithDynamics => $"{reportWithDynamics.Title} {reportWithDynamics.CreatedAt:yyyy-MM-dd-HH-mm}{extension}")
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

		private void ConfigureTreeView()
		{
			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;

			if(ViewModel.Report is null)
			{
				return;
			}

			if(ViewModel.Report.Value.IsT0)
			{
				var rowColumnsConfig = ytreeReportIndicatorsRows.CreateFluentColumnsConfig<SalesBySubdivisionsAnalitycsReport.DisplayRow>();

				rowColumnsConfig
					.AddColumn("")
					.AddTextRenderer(row =>
						row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.HeaderRow)
						|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.SubHeaderRow)
						|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.SubTotalRow)
						|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.TotalRow)
						? $"<b>{row.Title}</b>"
						: row.Title,
						useMarkup: true);

				if(ViewModel.Report?.Match(
					report => report.DisplayRows.Any(),
					reportWithDynamics => reportWithDynamics.DisplayRows.Any()) ?? false)
				{
					var count = ViewModel.Report?.Match(
						report => report.DisplayRows.First().DynamicColumns.Count,
						reportWithDynamics => reportWithDynamics.DisplayRows.First().DynamicColumns.Count);

					for(var i = 0; i < count; i++)
					{
						int index = i;
						rowColumnsConfig
							.AddColumn("")
							.AddTextRenderer(row =>
								row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.HeaderRow)
								|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.SubHeaderRow)
								|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.SubTotalRow)
								|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsReport.TotalRow)
								? $"<b>{row.DynamicColumns[index]}</b>"
								: row.DynamicColumns[index],
								useMarkup: true)
							.XAlign(1);
					}
				}

				rowColumnsConfig.AddColumn("").Finish();
			}
			else
			{
				var rowColumnsConfig = ytreeReportIndicatorsRows.CreateFluentColumnsConfig<SalesBySubdivisionsAnalitycsWithDynamicsReport.DisplayRow>();

				rowColumnsConfig
					.AddColumn("")
					.AddTextRenderer(row =>
						row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.HeaderRow)
						|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.SubHeaderRow)
						|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.SubTotalRow)
						|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.TotalRow)
						? $"<b>{row.Title}</b>"
						: row.Title,
						useMarkup: true);

				if(ViewModel.Report?.Match(
					report => report.DisplayRows.Any(),
					reportWithDynamics => reportWithDynamics.DisplayRows.Any()) ?? false)
				{
					var count = ViewModel.Report?.Match(
						report => report.DisplayRows.First().DynamicColumns.Count,
						reportWithDynamics => reportWithDynamics.DisplayRows.First().DynamicColumns.Count);

					for(var i = 0; i < count; i++)
					{
						int index = i;
						rowColumnsConfig
							.AddColumn("")
							.AddTextRenderer(row =>
								row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.HeaderRow)
								|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.SubHeaderRow)
								|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.SubTotalRow)
								|| row.GetType() == typeof(SalesBySubdivisionsAnalitycsWithDynamicsReport.TotalRow)
								? $"<b>{row.DynamicColumns[index]}</b>"
								: row.DynamicColumns[index],
								useMarkup: true)
							.XAlign(1);
					}
				}

				rowColumnsConfig.AddColumn("").Finish();
			}
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new SelectableParameterReportFilterView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.Show();
		}

		private void ShowReport()
		{
			try
			{
				ConfigureTreeView();
				ViewModel.Report?.Switch(
					report => ytreeReportIndicatorsRows.ItemsDataSource = report.DisplayRows,
					reportWithDynamics => ytreeReportIndicatorsRows.ItemsDataSource = reportWithDynamics.DisplayRows);
				ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
			}
			catch(Exception)
			{
				throw;
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.Report):
					ShowReport();
					break;
				default:
					break;
			}
		}
	}
}
