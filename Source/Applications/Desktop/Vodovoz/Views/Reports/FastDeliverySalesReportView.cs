using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.Threading.Tasks;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;

namespace Vodovoz.Views.Reports
{
	public partial class FastDeliverySalesReportView : TabViewBase<FastDeliverySalesReportViewModel>
	{
		public FastDeliverySalesReportView(FastDeliverySalesReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			rangepickerOrderCreateDate.Binding.AddSource(ViewModel)
				.AddBinding(ViewModel, vm => vm.CreateDateFrom, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.CreateDateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			ybtnRunReport.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsRunning, w => w.Sensitive)
				 .InitializeFromSource();

			ybtnExport.Binding
				.AddBinding(ViewModel, vm => vm.IsHasRows, w => w.Sensitive)
				.InitializeFromSource();

			ylblProgress.Binding
				.AddBinding(ViewModel, vm => vm.ProgressText, w => w.LabelProp)
				.InitializeFromSource();

			ybtnRunReport.Clicked += OnYbtnRunReportClicked;
			ybtnExport.Clicked += (sender, args) => ViewModel.ExportCommand.Execute();

			ConfigureReportTreeView();
		}

		private async void OnYbtnRunReportClicked(object sender, System.EventArgs e)
		{
			await Task.Run(() =>
			{
				try
				{
					ViewModel.GenerateCommand.Execute();
				}
				catch(Exception ex)
				{
					Application.Invoke((s, eventArgs) => throw ex);
				}

				Application.Invoke((s, a) =>
				{
					ytreeviewReport.ItemsDataSource = ViewModel.Report.Rows;
					ytreeviewReport.YTreeModel.EmitModelChanged();
				});
			});
		}

		private void ConfigureReportTreeView()
		{
			ytreeviewReport.ColumnsConfig = FluentColumnsConfig<FastDeliverySalesReportRow>.Create()
				.AddColumn("Номер заказа").AddNumericRenderer(n => n.OrderId)
				.AddColumn("Дата создания заказа").AddTextRenderer(n => n.OrderCreateDate)
				.AddColumn("Время создания заказа").AddTextRenderer(n => n.OrderCreateTime)
				.AddColumn("Номер МЛ").AddNumericRenderer(n => n.RouteListId)
				.AddColumn("ФИО водителя").AddTextRenderer(n => n.DriverNameWithInitials)
				.AddColumn("Район доставки").AddTextRenderer(n => n.District)
				.AddColumn("Дата доставки").AddTextRenderer(n => n.DeliveredDate)
				.AddColumn("Время доставки").AddTextRenderer(n => n.DeliveredTime)
				.AddColumn("Номенклатура").AddTextRenderer(n => n.Nomenclature)
				.AddColumn("Кол-во").AddNumericRenderer(n => n.Amount).Digits(2)
				.AddColumn("Сумма").AddNumericRenderer(n => n.Sum).Digits(2)
				.AddColumn("")
				.Finish();
		}
	}
}
