using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class MarketingReportView : TabViewBase<MarketingReportViewModel>
	{
		private IncludeExludeFiltersView _filterView;
		private Task _generationTask;

		public MarketingReportView(MarketingReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
			UpdateSliderArrow();
		}

		private void ConfigureDlg()
		{
			btnReportInfo.Clicked += (s, e) => ViewModel.ShowInfoCommand.Execute();

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
				.InitializeFromSource();

			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumGrouping.ItemsEnum = typeof(MarketingReportGroupingType);
			yenumGrouping.ShowSpecialStateAll = false;
			yenumGrouping.Binding
				.AddBinding(ViewModel, vm => vm.GroupingType, w => w.SelectedItem)
				.InitializeFromSource();

			yenumDateType.ItemsEnum = typeof(MarketingReportDateType);
			yenumDateType.ShowSpecialStateAll = false;
			yenumDateType.Binding
				.AddBinding(ViewModel, vm => vm.DateType, w => w.SelectedItem)
				.InitializeFromSource();

			ShowFilter();
			ytreeReportIndicatorsRows.RowActivated += OnReportRowActivated;
			ViewModel.PropertyChanged += ViewModelPropertyChanged;
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
			hpaned1.Position = 500;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.FilterViewModel))
			{
				ShowFilter();
			}

			if(e.PropertyName == nameof(ViewModel.Report))
			{
				ShowReport();
			}
		}

		private void ShowReport()
		{
			var columnsConfig = FluentColumnsConfig<MarketingReportDisplayRow>.Create();
			columnsConfig.AddColumn("Показатель")
				.AddTextRenderer(row => row.IsSection ? $"<b>{row.Title}</b>" : row.Title, useMarkup: true)
				.WrapWidth(350);
			columnsConfig.AddColumn("Значение")
				.AddTextRenderer(row => row.Value)
				.XAlign(1);
			columnsConfig.AddColumn("Дополнительно")
				.AddTextRenderer(row => row.AdditionalValue ?? string.Empty)
				.XAlign(1);
			columnsConfig.AddColumn("");

			ytreeReportIndicatorsRows.ColumnsConfig = columnsConfig.Finish();
			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;
			ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report?.DisplayRows;
			ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.HeightRequest = ViewModel.FilterViewModel.Filters.Count * 21 + 70;
			_filterView.Show();
		}

		private void OnReportRowActivated(object o, RowActivatedArgs args)
		{
			var row = ytreeReportIndicatorsRows.GetSelectedObject<MarketingReportDisplayRow>();
			if(row == null)
			{
				return;
			}

			GetClipboard(Gdk.Selection.Clipboard).Text = $"{row.Title}\t{row.Value}\t{row.AdditionalValue}";
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
					Gtk.Application.Invoke((s, eventArgs) => ViewModel.Report = report);
				}
				catch(OperationCanceledException)
				{
					// Отмена формирования отчета пользователем
				}
				catch(Exception ex)
				{
					Gtk.Application.Invoke((s, eventArgs) => { throw ex; });
				}
				finally
				{
					Gtk.Application.Invoke((s, eventArgs) => ViewModel.IsGenerating = false);
				}
			}, ViewModel.ReportGenerationCancelationTokenSource.Token);

			await _generationTask;
		}

		private void OnButtonAbortCreateReportClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancelationTokenSource?.Cancel();
		}

		protected async void OnYbuttonSaveClicked(object sender, EventArgs e)
		{
			const string extension = ".xlsx";
			var filechooser = new FileChooserDialog("Сохранить отчет...",
				null,
				FileChooserAction.Save,
				"Отменить", ResponseType.Cancel,
				"Сохранить", ResponseType.Accept)
			{
				DoOverwriteConfirmation = true,
				CurrentName = $"{Tab.TabName} {ViewModel.Report.CreatedAt:yyyy-MM-dd-HH-mm}{extension}"
			};

			var excelFilter = new FileFilter { Name = $"Документ Microsoft Excel ({extension})" };
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

		private void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
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
			ViewModel.PropertyChanged -= ViewModelPropertyChanged;
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			base.Dispose();
		}
	}
}
