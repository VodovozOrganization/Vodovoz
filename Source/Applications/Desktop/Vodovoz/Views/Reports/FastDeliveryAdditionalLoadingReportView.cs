using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.Threading.Tasks;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using static Vodovoz.ViewModels.ViewModels.Reports.FastDelivery.FastDeliveryAdditionalLoadingReportViewModel.FastDeliveryAdditionalLoadingReport;

namespace Vodovoz.Views.Reports
{
	public partial class FastDeliveryAdditionalLoadingReportView : TabViewBase<FastDeliveryAdditionalLoadingReportViewModel>
	{
		public FastDeliveryAdditionalLoadingReportView(FastDeliveryAdditionalLoadingReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			rangepickerRouteListCreateDate.Binding.AddSource(ViewModel)
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

			ybtnGenerateFastDeliveryRemainingBottlesReport.Clicked += (s, e) => ViewModel.GenerateFastDeliveryRemainingBottlesReportCommand.Execute();
			ybtnGenerateFastDeliveryRemainingBottlesReport.Binding
				.AddBinding(ViewModel, vm => vm.IsHasRows, w => w.Sensitive)
				.InitializeFromSource();

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
			ytreeviewReport.ColumnsConfig = FluentColumnsConfig<FastDeliveryAdditionalLoadingReportRow>.Create()
				.AddColumn("Дата").AddTextRenderer(n => n.RouteListDateString)
				.AddColumn("Номер МЛ").AddNumericRenderer(n => n.RouteListId)
				.AddColumn("Номинальных адресов").AddNumericRenderer(n => n.OwnOrdersCount)
				.AddColumn("Номенклатура").AddTextRenderer(n => n.AdditionaLoadingNomenclature)
				.AddColumn("Дозагруз (кол-во)").AddNumericRenderer(n => n.AdditionaLoadingAmount).Digits(2)
				.AddColumn("")
				.Finish();
		}
	}
}
