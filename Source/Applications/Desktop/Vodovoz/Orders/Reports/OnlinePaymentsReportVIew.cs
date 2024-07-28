using Gamma.GtkWidgets;
using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.ViewModels.Orders.Reports;

namespace Vodovoz.Orders.Reports
{
	[ToolboxItem(true)]
	public partial class OnlinePaymentsReportView : DialogViewBase<OnlinePaymentsReportViewModel>
	{
		public OnlinePaymentsReportView(OnlinePaymentsReportViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			daterangepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsDateTimeRangeCustomPeriod, w => w.Sensitive)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			yradiobuttonYesterday.Binding
				.AddBinding(ViewModel, vm => vm.IsDateTimeRangeYesterday, w => w.Active)
				.InitializeFromSource();

			yradiobuttonLast3Days.Binding
				.AddBinding(ViewModel, vm => vm.IsDateTimeRangeLast3Days, w => w.Active)
				.InitializeFromSource();

			yradiobuttonCustomPeriod.Binding
				.AddBinding(ViewModel, vm => vm.IsDateTimeRangeCustomPeriod, w => w.Active)
				.InitializeFromSource();

			speciallistcomboboxShop.ShowSpecialStateAll = true;
			speciallistcomboboxShop.SetRenderTextFunc<string>(o =>
				string.IsNullOrWhiteSpace(o) ? "{ нет названия }" : o);

			speciallistcomboboxShop.ItemsList = ViewModel.Shops;
			speciallistcomboboxShop.Binding.AddBinding(ViewModel, vm => vm.SelectedShop, w => w.SelectedItem)
				.InitializeFromSource();

			ybuttonGenerate.BindCommand(ViewModel.GenerateReportCommand);

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				ConfigureOrderRowTreeView(ytreeReportPaidRows);
				ytreeReportPaidRows.ItemsDataSource = ViewModel.PaidOrders;

				ConfigureOrderRowTreeView(ytreeReportPaidMissingRows);
				ytreeReportPaidMissingRows.ItemsDataSource = ViewModel.PaymentMissingOrders;

				ConfigureOrderRowTreeView(ytreeReportOverpaidRows);
				ytreeReportOverpaidRows.ItemsDataSource = ViewModel.OverpaidOrders;

				ConfigureOrderRowTreeView(ytreeReportUnderpaidRows);
				ytreeReportUnderpaidRows.ItemsDataSource = ViewModel.UnderpaidOrders;
			}
		}

		private void ConfigureOrderRowTreeView(yTreeView ytreeReportPaidRows)
		{
			ytreeReportPaidRows
				.CreateFluentColumnsConfig<OnlinePaymentsReport.Row>()
				.AddColumn("Дата заказа")
					.AddDateRenderer(r => r.OrderDeliveryDate)
				.AddColumn("Номер заказа")
					.AddNumericRenderer(r => r.OrderId)
				.AddColumn("Клиент")
					.AddTextRenderer(r => r.CcounterpartyFullName)
				.AddColumn("Адрес доставки")
					.AddTextRenderer(r => r.Address)
				.AddColumn("Номер оплаты и магазин")
					.AddTextRenderer(r => r.NumberAndShop)
				.AddColumn("Сумма заказа и оплачено клиентом")
					.AddTextRenderer(r =>
						r.ReportPaymentStatusEnum == OnlinePaymentsReport.Row.ReportPaymentStatus.Missing
						? r.OrderTotalSum.ToString("# ##0.##")
						: $"{r.TotalSumFromBank:# ##0.##} из {r.OrderTotalSum:# ##0.##}")
				.AddColumn("Статус заказа")
					.AddEnumRenderer(r => r.OrderStatus)
				.AddColumn("Автор заказа")
					.AddTextRenderer(r => r.Author)
				.AddColumn("Дата оплаты")
					.AddTextRenderer(r => r.PaymentDateTimeOrError)
				.Finish();
		}
	}
}
