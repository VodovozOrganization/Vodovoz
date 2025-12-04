using System;
using System.Threading.Tasks;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics;

namespace Vodovoz.Views.Reports
{
	public partial class DriversWarehousesEventsReportView : TabViewBase<DriversWarehousesEventsReportViewModel>
	{
		public DriversWarehousesEventsReportView(DriversWarehousesEventsReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			buttonLoad.Clicked += OnLoadReportClicked;
			buttonAbort.Clicked += OnAbortLoadReportClicked;
			buttonExport.Clicked += OnExportReportClicked;
			
			buttonLoad.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsGenerating, w => w.Visible)
				.InitializeFromSource();
			
			buttonAbort.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsGenerating, w => w.Visible)
				.AddBinding(vm => vm.IsGenerating, w => w.Sensitive)
				.InitializeFromSource();

			dateEventsRangePicker.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			dateGroupRadioButton.Binding
				.AddBinding(ViewModel, vm => vm.IsDateGroup, w => w.Active)
				.InitializeFromSource();
			
			driverGroupRadioButton.Binding
				.AddBinding(ViewModel, vm => vm.IsDriverGroup, w => w.Active)
				.InitializeFromSource();
			
			carGroupRadioButton.Binding
				.AddBinding(ViewModel, vm => vm.IsCarGroup, w => w.Active)
				.InitializeFromSource();
			
			lblDriver.LabelProp = DriversWarehousesEventsReportViewModel.EmloyeeTitle;
			carEntry.ViewModel = ViewModel.CarViewModel;
			driverEntry.ViewModel = ViewModel.DriverViewModel;
			firstEventNameEntry.ViewModel = ViewModel.StartEventNameViewModel;
			secondEventNameEntry.ViewModel = ViewModel.EndEventNameViewModel;
			
			eventBoxArrow.ButtonPressEvent += (o, args) =>
			{
				vboxFilters.Visible = !vboxFilters.Visible;
				arrowSlider.ArrowType = vboxFilters.Visible ? ArrowType.Left : ArrowType.Right;
			};

			ConfigureTreeViewReport();
		}
		
		private void ConfigureTreeViewReport()
		{
			treeViewReport.ColumnsConfig = FluentColumnsConfig<DriversWarehousesEventsReportNode>.Create()
				.AddColumn(DriversWarehousesEventsReportViewModel.DateTitle)
					.AddTextRenderer(x => x.EventDate.ToShortDateString())
				.AddColumn(DriversWarehousesEventsReportViewModel.EmloyeeTitle)
					.AddTextRenderer(x => x.DriverFio)
				.AddColumn(DriversWarehousesEventsReportViewModel.CarTitle)
					.AddTextRenderer(x => x.CarModelWithNumber)
				.AddColumn(DriversWarehousesEventsReportViewModel.FirstEventTitle)
					.AddTextRenderer(x => x.FirstEventName)
				.AddColumn(DriversWarehousesEventsReportViewModel.DocumentTypeColumn)
					.AddTextRenderer(x => x.FirstEventDocumentType)
				.AddColumn(DriversWarehousesEventsReportViewModel.DocumentNumberColumn)
					.AddTextRenderer(x => x.FirstEventDocumentNumber.ToString())
				.AddColumn(DriversWarehousesEventsReportViewModel.EventDistanceTitle)
					.AddNumericRenderer(x => x.FirstEventDistance)
				.AddColumn(DriversWarehousesEventsReportViewModel.EventTimeTitle)
					.AddTextRenderer(x => x.FirstEventTime.HasValue ? x.FirstEventTime.Value.ToString() : "")
				.AddColumn(DriversWarehousesEventsReportViewModel.SecondEventTitle)
					.AddTextRenderer(x => x.SecondEventName)
				.AddColumn(DriversWarehousesEventsReportViewModel.DocumentTypeColumn)
					.AddTextRenderer(x => x.SecondEventDocumentType)
				.AddColumn(DriversWarehousesEventsReportViewModel.DocumentNumberColumn)
					.AddTextRenderer(x => x.SecondEventDocumentNumber.ToString())
				.AddColumn(DriversWarehousesEventsReportViewModel.EventDistanceTitle)
					.AddNumericRenderer(x => x.SecondEventDistance)
				.AddColumn(DriversWarehousesEventsReportViewModel.EventTimeTitle)
					.AddTextRenderer(x => x.SecondEventTime.HasValue ? x.SecondEventTime.Value.ToString() : "")
				.AddColumn("")
				.Finish();

			//treeViewReport.ItemsDataSource = ViewModel.ReportNodes;
		}

		private async void OnLoadReportClicked(object sender, EventArgs e)
		{
			if(ViewModel.FirstEvent is null || ViewModel.SecondEvent is null)
			{
				ViewModel.ShowWarning("Для работы отчета надо выбрать оба события!");
				return;
			}
			
			ViewModel.IsGenerating = true;

			var task = Task.Run(async () =>
			{
				try
				{
					await ViewModel.GenerateReport();
					treeViewReport.ItemsDataSource = ViewModel.ReportNodes;
				}
				catch(OperationCanceledException)
				{
					Gtk.Application.Invoke((s, eventArgs) =>
					{
						ViewModel.ShowWarning("Формирование отчета было прервано");
					});
				}
				catch(Exception ex)
				{
					Gtk.Application.Invoke((s, eventArgs) => throw ex);
				}
				finally
				{
					Gtk.Application.Invoke((s, eventArgs) =>
					{
						ViewModel.IsGenerating = false;
					});
				}
			});

			await task;
		}

		private void OnAbortLoadReportClicked(object sender, EventArgs e)
		{
			ViewModel.AbortGenerateReportCommand.Execute();
		}

		private void OnExportReportClicked(object sender, EventArgs e)
		{
			ViewModel.ExportReport();
		}
	}
}
