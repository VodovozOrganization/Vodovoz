using System;
using System.ComponentModel;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Payments;

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
			btnReadFile.Clicked += (sender, args) => ViewModel.ParseCommand.Execute();

			btnReadFile.Binding
				.AddBinding(ViewModel, vm => vm.CanReadFile, w => w.Sensitive)
				.InitializeFromSource();

			GenerateButtons();

			ConfigureFileChooser();
			ConfigureClientsWidget();
			ConfigureOrdersTree();
			ConfigurePaymentsTree();
			
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}
		
		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Clients))
			{
				counterpartiesCmb.ItemsList = ViewModel.Clients;
			}
		}

		private void GenerateButtons()
		{
			var getClientsBtn = new yButton();
			getClientsBtn.Label = "Подобрать клиентов по ИНН из файла";
			getClientsBtn.Clicked += (sender, args) => ViewModel.GetClientsCommand.Execute();
			getClientsBtn.Binding
				.AddBinding(ViewModel, vm => vm.CanGetClient, w => w.Sensitive)
				.InitializeFromSource();
			
			getClientsBtn.Show();
			
			var processingDataBtn = new yButton();
			processingDataBtn.Label = "Обработать полученные данные по выбранному клиенту";
			processingDataBtn.Clicked += (sender, args) => ViewModel.ProcessingDataCommand.Execute();
			processingDataBtn.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedClient != null, w => w.Sensitive)
				.InitializeFromSource();
			
			processingDataBtn.Show();
			
			hboxFileManagment.Add(getClientsBtn);
			var box1 = (Box.BoxChild)hboxFileManagment[getClientsBtn];
			box1.Expand = false;
			hboxFileManagment.Add(processingDataBtn);
			var box2 = (Box.BoxChild)hboxFileManagment[processingDataBtn];
			box2.Expand = false;
		}

		private void ConfigureClientsWidget()
		{
			counterpartiesCmb.SetRenderTextFunc<Counterparty>(x => x.Name);
			counterpartiesCmb.Binding
				.AddBinding(ViewModel, vm => vm.SelectedClient, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void ConfigureFileChooser()
		{
			var xmlFilter = new FileFilter();
			xmlFilter.AddPattern(_xmlPattern);
			xmlFilter.Name = $"Файлы XML ({_xmlPattern})";
			var csvFilter = new FileFilter();
			csvFilter.AddPattern(_csvPattern);
			csvFilter.Name = $"Файлы CSV ({_csvPattern})";
			
			fileChooser.AddFilter(csvFilter);
			fileChooser.AddFilter(xmlFilter);
			
			fileChooser.Binding
				.AddBinding(ViewModel, vm => vm.SelectedFileName, w => w.Filename)
				.InitializeFromSource();
		}

		private void ConfigureOrdersTree()
		{
			treeOrders.ColumnsConfig = FluentColumnsConfig<OrderDiscrepanciesNode>.Create()
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
				.Finish();
			
			treeOrders.ItemsDataSource = ViewModel.OrdersNodes;
		}

		private void ConfigurePaymentsTree()
		{
			treePayments.ColumnsConfig = FluentColumnsConfig<PaymentDiscrepanciesNode>.Create()
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
			
			treePayments.ItemsDataSource = ViewModel.PaymentsNodes;
		}
	}
}
