using Gtk;
using QS.Views.GtkUI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using static Vodovoz.ViewModels.ViewModels.Reports.FastDelivery.FastDeliveryPercentCoverageReport;

namespace Vodovoz.Views.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FastDeliveryPercentCoverageReportView : TabViewBase<FastDeliveryPercentCoverageReportViewModel>
	{
		private Task _generationTask;

		public FastDeliveryPercentCoverageReportView(FastDeliveryPercentCoverageReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();

			UpdateSliderArrow();
		}

		private void Configure()
		{
			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			yspeccomboboxHoursFrom.ItemsList = ViewModel.Hours;

			yspeccomboboxHoursFrom.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartHour, w => w.SelectedItem)
				.InitializeFromSource();

			yspeccomboboxHoursTo.ItemsList = ViewModel.Hours;

			yspeccomboboxHoursTo.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EndHour, w => w.SelectedItem)
				.InitializeFromSource();

			ConfigurePreview();

			ybuttonCreateReport.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsGenerating, w => w.Visible)
				.AddBinding(vm => vm.CanGenerate, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsGenerating, w => w.Visible)
				.AddBinding(vm => vm.CanCancelGenerate, w => w.Sensitive)
				.InitializeFromSource();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
			ybuttonCreateReport.Clicked += OnButtonCreateReportClicked;
			ybuttonAbortCreateReport.Clicked += OnButtonAbortCreateReportClicked;
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
			ybuttonSave.Clicked += OnYbuttonSaveClicked;
		}

		private void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxFastDeliveryPercentCoverageReportFilterContainer.Visible = !vboxFastDeliveryPercentCoverageReportFilterContainer.Visible;
			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxFastDeliveryPercentCoverageReportFilterContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}

		private async void OnButtonCreateReportClicked(object sender, EventArgs e)
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

		private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				UpdateReport();
			}
		}

		private void ConfigurePreview()
		{
			ytreeReportIndicatorsRows.CreateFluentColumnsConfig<Row>()
				.AddColumn("Усредненные значения за период отчета")
					.AddTextRenderer(x => x.SubHeader)
				.AddColumn("Количество автомобилей")
					.AddNumericRenderer(x => x.CarsCount)
				.AddColumn("Радиус обслуживания")
					.AddNumericRenderer(x => x.ServiceRadius)
				.AddColumn("Процент покрытия")
					.AddNumericRenderer(x => x.PercentCoverage.ToString("P"))
				.AddColumn("")
				.Finish();
		}

		private void UpdateReport()
		{
			if(ViewModel.Report?.Grouping != null)
			{
				ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report?.Rows;

				ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
			}
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
				CurrentName = $"{Tab.TabName.Replace("\"", "")} {ViewModel.Report.CreatedAt:yyyy-MM-dd-HH-mm}{extension}"
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

		private void OnButtonAbortCreateReportClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancelationTokenSource.Cancel();
		}

		public override void Dispose()
		{
			ViewModel.PropertyChanged -= ViewModelPropertyChanged;
			ybuttonCreateReport.Clicked -= OnButtonCreateReportClicked;
			ybuttonAbortCreateReport.Clicked -= OnButtonAbortCreateReportClicked;
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			ybuttonSave.Clicked -= OnYbuttonSaveClicked;
			base.Dispose();
		}
	}
}
