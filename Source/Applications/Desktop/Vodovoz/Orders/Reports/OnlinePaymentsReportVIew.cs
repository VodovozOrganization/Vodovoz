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
			ybuttonGenerate.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.BindCommand(ViewModel.CancelGenerationCommand);
			ybuttonAbortCreateReport.Binding
				.AddFuncBinding(ViewModel, vm => !vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonSave.BindCommand(ViewModel.ExportReportCommand);

			ConfigureOrderRowTreeView(ytreeReportPaidRows);
			ConfigureOrderRowTreeView(ytreeReportPaidMissingRows);
			ConfigureOrderRowTreeView(ytreeReportOverpaidRows);
			ConfigureOrderRowTreeView(ytreeReportUnderpaidRows);

			ytreeReportPaymentsWithoutOrdersRows
				.CreateFluentColumnsConfig<OnlinePaymentsReport.PaymentWithoutOrderRow>()
				.AddColumn("Дата оплаты")
					.AddDateRenderer(r => r.DateTime)
				.AddColumn("Номер оплаты")
					.AddNumericRenderer(r => r.Number)
				.AddColumn("Магазин")
					.AddTextRenderer(r => r.Shop)
				.AddColumn("Сумма (р.)")
					.AddTextRenderer(r => $"{r.Sum:# ##0.00}")
				.AddColumn("Электронная почта")
					.AddTextRenderer(r => r.Email)
				.AddColumn("Номер телефона")
					.AddTextRenderer(r => r.Phone)
				.AddColumn("Клиент")
					.AddTextRenderer(r => r.CounterpartyFullName)
				.Finish();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				ytreeReportPaidRows.ItemsDataSource = ViewModel.PaidOrders;

				ytreeReportPaidMissingRows.ItemsDataSource = ViewModel.PaymentMissingOrders;

				ytreeReportOverpaidRows.ItemsDataSource = ViewModel.OverpaidOrders;

				ytreeReportUnderpaidRows.ItemsDataSource = ViewModel.UnderpaidOrders;

				ytreeReportPaymentsWithoutOrdersRows.ItemsDataSource = ViewModel.PaymentWithoutOrder;
			}
		}

		private void ConfigureOrderRowTreeView(yTreeView ytreeReportPaidRows)
		{
			ytreeReportPaidRows
				.CreateFluentColumnsConfig<OnlinePaymentsReport.OrderRow>()
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
					.AddTextRenderer(r => r.SumAndPaid)
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
