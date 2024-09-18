using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using QS.Views.Dialog;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis;
using static Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis.PaymentsDiscrepanciesAnalysisViewModel;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Payments
{
	public partial class PaymentsDiscrepanciesAnalysisView : DialogViewBase<PaymentsDiscrepanciesAnalysisViewModel>
	{
		private const string _xlsxPattern = "*.xlsx";

		private readonly Color _primaryTextColor = GdkColors.PrimaryText;
		private readonly Color _dangerTextColor = GdkColors.DangerText;

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

			datepickerCommonMaxDate.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CommonReconciliationDataMaxDate, w => w.DateOrNull)
				.AddBinding(vm => vm.CanReadFile, w => w.Sensitive)
				.InitializeFromSource();

			ylabelClientDebtDvData.Binding
				.AddBinding(ViewModel, vm => vm.BalanceInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientDebtDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.Balance1C, w => w.Text)
				.InitializeFromSource();

			ylabelClientOldBalanceDvData.Binding
				.AddBinding(ViewModel, vm => vm.OldBalanceInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientOldBalanceDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.OldBalance1C, w => w.Text)
				.InitializeFromSource();

			ylabelClientOrdersSumDvData.Binding
				.AddBinding(ViewModel, vm => vm.OrdersTotalSumInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientOrdersSumDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.OrdersTotalSum1C, w => w.Text)
				.InitializeFromSource();

			ylabelClientTotalPaymentsDvData.Binding
				.AddBinding(ViewModel, vm => vm.PaymentsTotalSumInDatabase, w => w.Text)
				.InitializeFromSource();

			ylabelClientTotalPaymentsDocumentData.Binding
				.AddBinding(ViewModel, vm => vm.PaymentsTotalSum1C, w => w.Text)
				.InitializeFromSource();

			speciallistcomboboxPaymentStatus.ShowSpecialStateAll = true;
			speciallistcomboboxPaymentStatus.SetRenderTextFunc<OrderPaymentStatus?>(x => x.GetEnumTitle());
			speciallistcomboboxPaymentStatus.ItemsList = Enum.GetValues(typeof(OrderPaymentStatus));
			speciallistcomboboxPaymentStatus.Binding
				.AddBinding(ViewModel, vm => vm.OrderPaymentStatus, w => w.SelectedItem)
				.InitializeFromSource();

			ycheckbuttonHideUnregisteredCounterparties.Binding
				.AddBinding(ViewModel, vm => vm.HideUnregisteredCounterparties, w => w.Active)
				.InitializeFromSource();

			ConfigureFileChooser();
			ConfigureOrdersTree();
			ConfigurePaymentsTree();
			ConfigureCounterpartiesTree();
		}

		private void ConfigureFileChooser()
		{
			var xlsxFilter = new FileFilter();
			xlsxFilter.AddPattern(_xlsxPattern);
			xlsxFilter.Name = $"Файлы XLSX ({_xlsxPattern})";

			yfilechooserbutton.AddFilter(xlsxFilter);

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
					.Digits(2)
				.AddColumn("Сумма по ДВ")
					.AddNumericRenderer(n => n.ProgramOrderSum)
					.Digits(2)
					.AddSetter((spin, node) =>
					{
						spin.ForegroundGdk = _primaryTextColor;

						if(node.OrderSumDiscrepancy)
						{
							spin.ForegroundGdk = _dangerTextColor;
						}
					})
				.AddColumn("Распределенная сумма")
					.AddNumericRenderer(n => n.AllocatedSum)
					.Digits(2)
				.AddColumn("Статус оплаты заказа")
					.AddTextRenderer(n => n.OrderPaymentStatus.HasValue
						? n.OrderPaymentStatus.GetEnumTitle()
						: string.Empty)
				.AddColumn("Клиент")
					.AddTextRenderer(n => n.OrderClientNameInDatabase)
					.AddSetter((spin, node) =>
					{
						spin.ForegroundGdk = _primaryTextColor;

						if(ViewModel.SelectedClient != null && node.OrderClientInnInDatabase != ViewModel.SelectedClient.INN)
						{
							spin.ForegroundGdk = _dangerTextColor;
						}
					})
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
				.AddColumn("Плательщик")
					.AddTextRenderer(n => n.PayerName)
				.AddColumn("Сумма по акту")
					.AddNumericRenderer(n => n.DocumentPaymentSum)
					.Digits(2)
				.AddColumn("Сумма по ДВ")
					.AddNumericRenderer(n => n.ProgramPaymentSum)
					.Digits(2)
				.AddColumn("Распределено на клиента")
					.AddTextRenderer(n => n.CounterpartyId.ToString())
					.AddTextRenderer(n => n.CounterpartyName)
					.AddSetter((spin, node) =>
					{
						spin.ForegroundGdk = _primaryTextColor;

						if(ViewModel.SelectedClient != null && node.CounterpartyInn != ViewModel.SelectedClient.INN)
						{
							spin.ForegroundGdk = _dangerTextColor;
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
				.AddColumn("ИНН")
					.AddNumericRenderer(n => n.CounterpartyInn)
				.AddColumn("Наименование")
					.AddTextRenderer(n => n.CounterpartyName)
					.WrapWidth(1200).WrapMode(WrapMode.WordChar)
				.AddColumn("Баланс по ДВ")
					.AddNumericRenderer(n => n.CounterpartyBalance)
					.Digits(2)
				.AddColumn("Баланс по 1С")
					.AddNumericRenderer(n => n.CounterpartyBalance1C)
					.Digits(2)
				.AddColumn("")
				.Finish();

			ytreeviewCounterpartiesData.ItemsDataSource = ViewModel.BalanceNodes;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Clients))
			{
				speciallistcomboboxClientInfo.ItemsList = null;
				speciallistcomboboxClientInfo.ItemsList = ViewModel.Clients;
			}
		}

		public override void Destroy()
		{
			ytreeviewOrdersData.Destroy();
			ytreeviewPaymentsData.Destroy();
			ytreeviewCounterpartiesData.Destroy();

			base.Destroy();
		}
	}
}
