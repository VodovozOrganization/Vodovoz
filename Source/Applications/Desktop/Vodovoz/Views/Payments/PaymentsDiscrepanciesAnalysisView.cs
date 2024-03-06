using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel.CounterpartySettlementsReconciliation;

namespace Vodovoz.Views.Payments
{
	public partial class PaymentsDiscrepanciesAnalysisView : DialogViewBase<PaymentsDiscrepanciesAnalysisViewModel>
	{
		private const string _xmlPattern = "*.xlsx";

		public PaymentsDiscrepanciesAnalysisView(PaymentsDiscrepanciesAnalysisViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			ynotebook.ShowTabs = false;
			ynotebook.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedCheckMode, w => w.CurrentPage)
				.InitializeFromSource();

			yradiobuttonCounterpartyMode.Toggled += (s, e) =>
			{
				if(yradiobuttonCounterpartyMode.Active)
				{
					ViewModel.SetByCounterpartyCheckModeCommand.Execute();
				}
			};

			yradiobuttonCommonMode.Toggled += (s, e) =>
			{
				if(yradiobuttonCommonMode.Active)
				{
					ViewModel.SetCommonReconciliationCheckModeCommand.Execute();
				}
			};

			ybuttonReadFile.Binding
				.AddBinding(ViewModel, vm => vm.CanReadFile, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonReadFile.Clicked += (s, e) => ViewModel.AnalyseDiscrepanciesCommand.Execute();

			ycheckbuttonClientDiscrepanciesOnly.Binding
				.AddBinding(ViewModel, vm => vm.IsDiscrepanciesOnly, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonCounterpartyClosedOnly.Binding
				.AddBinding(ViewModel, vm => vm.IsClosedOrdersOnly, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonExcludeOldData.Binding
				.AddBinding(ViewModel, vm => vm.IsExcludeOldData, w => w.Active)
				.InitializeFromSource();

			speciallistcomboboxClientInfo.SetRenderTextFunc<Counterparty>(x => x.Name);
			speciallistcomboboxClientInfo.Binding
				.AddBinding(ViewModel, vm => vm.SelectedClient, w => w.SelectedItem)
				.InitializeFromSource();

			ylabelClientDebtDvData.Binding
				.AddBinding(ViewModel, vm => vm.TotalDebtInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientDebtDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.TotalDebtInFile, w => w.Text)
				.InitializeFromSource();

			ylabelClientOldBalanceDvData.Binding
				.AddBinding(ViewModel, vm => vm.OldDebtInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientOldBalanceDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.OldDebtInFile, w => w.Text)
				.InitializeFromSource();

			ylabelClientOrdersSumDvData.Binding
				.AddBinding(ViewModel, vm => vm.OrdersTotalSumInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientOrdersSumDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.OrdersTotalSumInFile, w => w.Text)
				.InitializeFromSource();

			ylabelClientTotalPaymentsDvData.Binding
				.AddBinding(ViewModel, vm => vm.PaymentsTotalSumInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientTotalPaymentsDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.PaymentsTotalSumInFile, w => w.Text)
				.InitializeFromSource();

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

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Clients))
			{
				speciallistcomboboxClientInfo.ItemsList = ViewModel.Clients;
			}
		}
	}
}
