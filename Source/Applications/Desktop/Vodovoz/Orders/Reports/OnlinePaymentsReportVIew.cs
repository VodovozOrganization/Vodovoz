using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Orders.Reports;

namespace Vodovoz.Orders.Reports
{
	[ToolboxItem(true)]
	public partial class OnlinePaymentsReportView : DialogViewBase<OnlinePaymentsReportViewModel>
	{
		private int _hpanedDefaultPosition = 428;
		private int _hpanedMinimalPosition = 16;

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

			ConfigureFutureOrdersRowsView();
			ConfigureCurrentOrdersRowsView();
			ConfigureTreeViews();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			UpdateSliderArrow();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void ConfigureTreeViews()
		{
			ConfigureOrderRowTreeView(ytreeReportFuturePaidRows, GdkColors.PrimaryBase);
			ConfigureOrderRowTreeView(ytreeReportFuturePaidMissingRows, GdkColors.DangerBase);
			ConfigureOrderRowTreeView(ytreeReportFutureOverpaidRows,
				GdkColors.SuccessBase);
			ConfigureOrderRowTreeView(ytreeReportFutureUnderpaidRows, GdkColors.WarningBase);

			ConfigureOrderRowTreeView(ytreeReportPaidRows, GdkColors.PrimaryBase);
			ConfigureOrderRowTreeView(ytreeReportPaidMissingRows, GdkColors.DangerBase);
			ConfigureOrderRowTreeView(ytreeReportOverpaidRows,
				GdkColors.SuccessBase);
			ConfigureOrderRowTreeView(ytreeReportUnderpaidRows, GdkColors.WarningBase);

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

			ytreeReportPaymentsWithoutOrdersRows.Binding
				.AddBinding(ViewModel, vm => vm.HasPaymentWithoutOrder, w => w.Visible)
				.InitializeFromSource();

			ylabelPaymentsWithoutOrders.Binding
				.AddBinding(ViewModel, vm => vm.HasPaymentWithoutOrder, w => w.Visible)
				.InitializeFromSource();
		}

		private void ConfigureCurrentOrdersRowsView()
		{
			ylabelPaid.Binding
				.AddBinding(ViewModel, vm => vm.HasPaidOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportPaidRows.Binding
				.AddBinding(ViewModel, vm => vm.HasPaidOrders, w => w.Visible)
				.InitializeFromSource();

			ylabelPaidMissing.Binding
				.AddBinding(ViewModel, vm => vm.HasPaymentMissingOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportPaidMissingRows.Binding
				.AddBinding(ViewModel, vm => vm.HasPaymentMissingOrders, w => w.Visible)
				.InitializeFromSource();

			ylabelOverpaid.Binding
				.AddBinding(ViewModel, vm => vm.HasOverpaidOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportOverpaidRows.Binding
				.AddBinding(ViewModel, vm => vm.HasOverpaidOrders, w => w.Visible)
				.InitializeFromSource();

			ylabelUnderpaid.Binding
				.AddBinding(ViewModel, vm => vm.HasUnderpaidOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportUnderpaidRows.Binding
				.AddBinding(ViewModel, vm => vm.HasUnderpaidOrders, w => w.Visible)
				.InitializeFromSource();

			yvboxCurrentOrders.Binding
				.AddBinding(ViewModel, vm => vm.HasAnyOrders, w => w.Visible)
				.InitializeFromSource();
		}

		private void ConfigureFutureOrdersRowsView()
		{
			ylabelFuturePaid.Binding
				.AddBinding(ViewModel, vm => vm.HasFuturePaidOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportFuturePaidRows.Binding
				.AddBinding(ViewModel, vm => vm.HasFuturePaidOrders, w => w.Visible)
				.InitializeFromSource();

			ylabelFuturePaidMissing.Binding
				.AddBinding(ViewModel, vm => vm.HasFuturePaymentMissingOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportFuturePaidMissingRows.Binding
				.AddBinding(ViewModel, vm => vm.HasFuturePaymentMissingOrders, w => w.Visible)
				.InitializeFromSource();

			ylabelFutureOverpaid.Binding
				.AddBinding(ViewModel, vm => vm.HasFutureOverpaidOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportFutureOverpaidRows.Binding
				.AddBinding(ViewModel, vm => vm.HasFutureOverpaidOrders, w => w.Visible)
				.InitializeFromSource();

			ylabelFutureUnderpaid.Binding
				.AddBinding(ViewModel, vm => vm.HasFutureUnderpaidOrders, w => w.Visible)
				.InitializeFromSource();

			ytreeReportFutureUnderpaidRows.Binding
				.AddBinding(ViewModel, vm => vm.HasFutureUnderpaidOrders, w => w.Visible)
				.InitializeFromSource();

			yvboxFutureOrders.Binding
				.AddBinding(ViewModel, vm => vm.HasAnyFutureOrders, w => w.Visible)
				.InitializeFromSource();
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

				ytreeReportFuturePaidRows.ItemsDataSource = ViewModel.FuturePaidOrders;

				ytreeReportFuturePaidMissingRows.ItemsDataSource = ViewModel.FuturePaymentMissingOrders;

				ytreeReportFutureOverpaidRows.ItemsDataSource = ViewModel.FutureOverpaidOrders;

				ytreeReportFutureUnderpaidRows.ItemsDataSource = ViewModel.FutureUnderpaidOrders;

				ytreeReportPaymentsWithoutOrdersRows.ItemsDataSource = ViewModel.PaymentWithoutOrder;
			}
		}

		private void ConfigureOrderRowTreeView(yTreeView ytreeReportPaidRows, Gdk.Color baseColor)
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
				.RowCells()
				.AddSetter<CellRenderer>((cell, row) => cell.CellBackgroundGdk = baseColor)
				.Finish();
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxOblinePaymentsReportParameters.Visible = !vboxOblinePaymentsReportParameters.Visible;

			hpaned1.Position = vboxOblinePaymentsReportParameters.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxOblinePaymentsReportParameters.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
