using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Payments;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysisViewModel;

namespace Vodovoz.Views.Payments
{
	public partial class PaymentsDiscrepanciesAnalysisView : DialogViewBase<PaymentsDiscrepanciesAnalysisViewModel>
	{
		private const string _xmlPattern = "*.xml";
		private const string _csvPattern = "*.csv";

		public PaymentsDiscrepanciesAnalysisView(PaymentsDiscrepanciesAnalysisViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ynotebook.ShowTabs = false;
			ynotebook.Binding
				.AddBinding(ViewModel, vm => vm.SelectedCheckMode, w => w.CurrentPage)
				.InitializeFromSource();

			ycheckbuttonClientDiscrepanciesOnly.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedCheckMode == DiscrepancyCheckMode.ByCounterparty, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonClientDiscrepanciesOnly.Toggled += (s, e) => ViewModel.SetByCounterpartyCheckModeCommand.Execute();

			ConfigureFileChooser();
			ConfigureOrdersTree();
			ConfigurePaymentsTree();
			ConfigureCounterpartiesTree();
		}

		private void ConfigureFileChooser()
		{
			var xmlFilter = new FileFilter();
			xmlFilter.AddPattern(_xmlPattern);
			xmlFilter.Name = $"Файлы XML ({_xmlPattern})";
			var csvFilter = new FileFilter();
			csvFilter.AddPattern(_csvPattern);
			csvFilter.Name = $"Файлы CSV ({_csvPattern})";

			yfilechooserbutton.AddFilter(csvFilter);
			yfilechooserbutton.AddFilter(xmlFilter);

			yfilechooserbutton.Binding
				.AddBinding(ViewModel, vm => vm.SelectedFileName, w => w.Filename)
				.InitializeFromSource();
		}

		private void ConfigureOrdersTree()
		{
			ytreeviewOrdersData.ColumnsConfig = FluentColumnsConfig<OrderDiscrepanciesNode>.Create()
				.AddColumn("№ заказа")
					.AddNumericRenderer(n => n.OrderId)
				.AddColumn("Дата заказа")
					.AddTextRenderer(n =>
						n.OrderDeliveryDate.HasValue ?
							n.OrderDeliveryDate.Value.ToShortDateString()
							: string.Empty)
				.AddColumn("Статус заказа")
					.AddTextRenderer(n => n.OrderStatus.HasValue ? n.OrderStatus.GetEnumTitle() : string.Empty)
				.AddColumn("Сумма по акту")
					.AddNumericRenderer(n => n.DocumentOrderSum)
				.AddColumn("Сумма по ДВ")
					.AddNumericRenderer(n => n.ProgramOrderSum)
					.AddSetter((spin, node) =>
					{
						spin.ForegroundGdk = GdkColors.PrimaryText;

						if(node.OrderSumDiscrepancy)
						{
							spin.ForegroundGdk = GdkColors.DangerText;
						}
					})
				.AddColumn("Распределенная сумма")
					.AddNumericRenderer(n => n.AllocatedSum)
				.AddColumn("Статус оплаты заказа")
					.AddTextRenderer(n => n.OrderPaymentStatus.HasValue
						? n.OrderPaymentStatus.GetEnumTitle()
						: string.Empty)
				/*.RowCells()
				.AddSetter<CellRenderer>((c, n) =>
				{
					c.CellBackgroundGdk = GdkColors.PrimaryBase;

					if(n.IsMissingFromDocument)
					{
						c.CellBackgroundGdk = GdkColors.Orange;
					}
				})*/
				.AddColumn("")
				.Finish();

			ytreeviewOrdersData.ItemsDataSource = ViewModel.OrdersNodes;
		}

		private void ConfigurePaymentsTree()
		{
			ytreeviewPaymentsData.ColumnsConfig = FluentColumnsConfig<PaymentDiscrepanciesNode>.Create()
				.AddColumn("№ платежа")
					.AddNumericRenderer(n => n.PaymentNum)
				.AddColumn("Дата платежа")
					.AddTextRenderer(n => n.PaymentDate.ToShortDateString())
				.AddColumn("Сумма по акту")
					.AddNumericRenderer(n => n.DocumentPaymentSum)
				.AddColumn("Сумма по ДВ")
					.AddNumericRenderer(n => n.ProgramPaymentSum)
				.AddColumn("Распределено на клиента")
					.AddTextRenderer(n => n.CounterpartyId.ToString())
					.AddTextRenderer(n => n.CounterpartyName)
					.AddSetter((spin, node) =>
					{
						spin.ForegroundGdk = GdkColors.PrimaryText;

						if(node.CounterpartyInn != ViewModel.SelectedClient.INN)
						{
							spin.ForegroundGdk = GdkColors.DangerText;
						}
					})
				.AddColumn("Назначение платежа")
					.AddTextRenderer(n => n.PaymentPurpose)
				.AddColumn("")
				.Finish();

			ytreeviewPaymentsData.ItemsDataSource = ViewModel.PaymentsNodes;
		}

		private void ConfigureCounterpartiesTree()
		{
			ytreeviewCounterpartiesData.ColumnsConfig = FluentColumnsConfig<CounterpartyBalanceNode>.Create()
				.AddColumn("Наименование")
					.AddTextRenderer(n => n.CounterpartyName)
				.AddColumn("Баланс по ДВ")
					.AddNumericRenderer(n => n.CounterpartyBalance)
				.AddColumn("Баланс по 1С")
					.AddNumericRenderer(n => n.CounterpartyBalance1C)
				.AddColumn("")
				.Finish();

			ytreeviewCounterpartiesData.ItemsDataSource = ViewModel.CounterpartyBalanceNodes;
		}
	}
}
