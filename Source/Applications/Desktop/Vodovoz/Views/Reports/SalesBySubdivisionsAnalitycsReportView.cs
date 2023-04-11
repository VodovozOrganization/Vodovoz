using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;
using static Vodovoz.ViewModels.ViewModels.Reports.Sales.SalesBySubdivisionsAnalitycsReport;

namespace Vodovoz.Views.Reports
{
	public partial class SalesBySubdivisionsAnalitycsReportView : TabViewBase<SalesBySubdivisionsAnalitycsReportViewModel>
	{
		private Task _generationTask;

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
				.AddBinding(vm => vm.FirstPeriodStartDate, w => w.StartDate)
				.AddBinding(vm => vm.FirstPeriodEndDate, w => w.EndDate)
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
				.AddBinding(ViewModel, vm => vm.SplitByWarehouses, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanSplitByWarehouse, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerate, w => w.Visible)
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

					Application.Invoke((s, eventArgs) => { ViewModel.Report = report; });
				}
				catch(OperationCanceledException)
				{
					Application.Invoke((s, eventArgs) => {
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
					Application.Invoke((s, eventArgs) => { throw ex; });
				}
				finally
				{
					Application.Invoke((s, eventArgs) => { ViewModel.IsGenerating = false; });
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

		private void ConfigureTreeView()
		{
			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;

			var rowColumnsConfig = ytreeReportIndicatorsRows.CreateFluentColumnsConfig<SalesBySubdivisionsAnalitycsReport.DisplayRow>();

			rowColumnsConfig
				.AddColumn("")
				.AddTextRenderer(row =>
					row.GetType() == typeof(HeaderRow)
					|| row.GetType() == typeof(SubHeaderRow)
					|| row.GetType() == typeof(SubTotalRow)
					|| row.GetType() == typeof(TotalRow)
					? $"<b>{row.Title}</b>"
					: row.Title,
					useMarkup: true);

			if(ViewModel.Report.DisplayRows.Any())
			{
				var count = ViewModel.Report.DisplayRows.First().DynamicColumns.Count;

				for(var i = 0; i < count; i++)
				{
					int index = i;
					rowColumnsConfig
						.AddColumn("")
						.AddTextRenderer(row =>
						row.GetType() == typeof(HeaderRow)
						|| row.GetType() == typeof(SubHeaderRow)
						|| row.GetType() == typeof(SubTotalRow)
						|| row.GetType() == typeof(TotalRow)
						? $"<b>{row.DynamicColumns[index]}</b>"
						: row.DynamicColumns[index],
						useMarkup: true);
				}
			}
			
			rowColumnsConfig.AddColumn("").Finish();
		}

		private void ShowReport()
		{
			try
			{
				ConfigureTreeView();
				ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report.DisplayRows;
				ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
			}
			catch(Exception e)
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
