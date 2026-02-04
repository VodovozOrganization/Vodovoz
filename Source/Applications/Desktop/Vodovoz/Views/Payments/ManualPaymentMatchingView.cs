using Gamma.ColumnConfig;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views
{
	[ToolboxItem(true)]
	public partial class ManualPaymentMatchingView : TabViewBase<ManualPaymentMatchingViewModel>
	{
		public ManualPaymentMatchingView(ManualPaymentMatchingViewModel manualPaymentLoaderViewModel)
			: base(manualPaymentLoaderViewModel)
		{
			Build();
			Configure();
		}

		void Configure()
		{
			notebook1.ShowTabs = false;

			#region Radio buttons

			radioBtnAllocateOrders.Active = true;
			radioBtnAllocateOrders.Toggled += RadioBtnAllocateOrdersOnToggled;
			radioBtnAllocatedOrders.Toggled += RadioBtnAllocatedOrdersOnToggled;
			radioBtnAllocatedOrders.Sensitive = ViewModel.HasPaymentItems;

			#endregion

			btnSave.BindCommand(ViewModel.SaveViewModelCommand);
			btnCancel.BindCommand(ViewModel.CloseCommand);
			buttonComplete.BindCommand(ViewModel.CompleteAllocationCommand);
			btnAddCounterparty.BindCommand(ViewModel.AddCounterpatyCommand);
			btnAddCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyIsNull, w => w.Sensitive)
				.InitializeFromSource();
			
			ybtnRevertPayment.BindCommand(ViewModel.RevertAllocatedSum);
			ybtnRevertPayment.Binding
				.AddBinding(ViewModel, vm => vm.CanRevertPay, w => w.Sensitive)
				.InitializeFromSource();

			daterangepicker1.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			daterangepicker1.PeriodChangedByUser += (sender, e) => ViewModel.UpdateNodes();

			enumchecklistOrdersStatuses.EnumType = typeof(OrderStatus);
			enumchecklistOrdersStatuses.Binding
				.AddBinding(ViewModel, vm => vm.OrderStatuses, w => w.SelectedValuesList, new EnumsListConverter<OrderStatus>())
				.InitializeFromSource();

			enumchecklistPaymentsStatuses.EnumType = typeof(OrderPaymentStatus);
			enumchecklistPaymentsStatuses.Binding
				.AddBinding(ViewModel, vm => vm.OrderPaymentStatuses, w => w.SelectedValuesList, new EnumsListConverter<OrderPaymentStatus>())
				.InitializeFromSource();

			labelTotalSum.Text = ViewModel.Entity.Total.ToString();
			labelLastBalance.Binding
				.AddBinding(ViewModel, vm => vm.LastBalance, w => w.Text, new DecimalToStringConverter())
				.InitializeFromSource();
			labelToAllocate.Binding
				.AddBinding(ViewModel, vm => vm.SumToAllocate, w => w.Text, new DecimalToStringConverter())
				.InitializeFromSource();

			ylabelCurBalance.Binding
				.AddBinding(ViewModel, vm => vm.CurrentBalance, v => v.Text, new DecimalToStringConverter())
				.InitializeFromSource();
			ylabelAllocated.Binding
				.AddBinding(ViewModel, vm => vm.AllocatedSum, v => v.Text, new DecimalToStringConverter())
				.InitializeFromSource();

			ylabelWaitForPaymentValue.Binding
				.AddFuncBinding(ViewModel, vm => vm.CounterpartyWaitingForPaymentOrdersDebt > 0 ? vm.CounterpartyWaitingForPaymentOrdersDebt.ToString("N2") : "0,00", w => w.Text)
				.InitializeFromSource();

			ylabelCloseDocumentsValue.Binding
				.AddFuncBinding(ViewModel, vm => vm.CounterpartyClosingDocumentsOrdersDebt > 0 ? vm.CounterpartyClosingDocumentsOrdersDebt.ToString("N2") : "0,00", w => w.Text)
				.InitializeFromSource();

			ylabelOtherOrdersDebtValue.Binding
				.AddFuncBinding(ViewModel, vm => vm.CounterpartyOtherOrdersDebt > 0 ? vm.CounterpartyOtherOrdersDebt.ToString("N2") : "0,00", w => w.Text)
				.InitializeFromSource();

			labelPayer.Text = ViewModel.Entity.CounterpartyName;
			labelPaymentNum.Text = ViewModel.Entity.PaymentNum.ToString();
			labelDate.Text = ViewModel.Entity.Date.ToShortDateString();

			ytextviewPaymentPurpose.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(e => e.PaymentPurpose, w => w.Buffer.Text)
				.AddBinding(e => e.IsManuallyCreated, w => w.Editable)
				.InitializeFromSource();

			ytextviewComments.Binding
				.AddBinding(ViewModel.Entity, vm => vm.Comment, v => v.Buffer.Text)
				.InitializeFromSource();

			var searchView = new SearchView((SearchViewModel)ViewModel.Search);
			hboxSearch.Add(searchView);
			searchView.Show();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			ConfigureTrees();
			ConfigureEntityEntries();
		}

		private void ConfigureEntityEntries()
		{
			var viewModel = new LegacyEEVMBuilderFactory<Payment>(
				ViewModel, null, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			viewModel.ChangedByUser += UpdateData;
			counterpartyEntry.ViewModel =  viewModel;
			counterpartyEntry.ViewModel.IsEditable = ViewModel.CanChangeCounterparty;
			
			profitCategoryEntry.ViewModel = ViewModel.ProfitCategoryEntryViewModel;
		}

		private void UpdateData(object sender, EventArgs e)
		{
			ViewModel.UpdateCMOCounterparty();
			ViewModel.UpdateNodes();
			ViewModel.GetLastBalance();
			ViewModel.UpdateSumToAllocate();
			ViewModel.UpdateCurrentBalance();
			ViewModel.GetCounterpartyDebt();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.CanChangeCounterparty))
			{
				counterpartyEntry.ViewModel.IsEditable = ViewModel.CanChangeCounterparty;
			}
		}

		private void ConfigureTrees()
		{
			ytreeviewOrdersAllocate.ColumnsConfig = FluentColumnsConfig<ManualPaymentMatchingViewModelNode>.Create()
				.AddColumn("№ заказа")
					.AddTextRenderer(node => node.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("№ УПД")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.UpdDocumentName)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Дата заказа")
					.AddTextRenderer(node => node.OrderDate.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Сумма заказа, р.")
					.AddTextRenderer(node => $"{node.ActualOrderSum}")
					.XAlign(0.5f)
				.AddColumn("Прошлые оплаты, р.")
					.AddNumericRenderer(node => node.LastPayments)
					.Digits(2)
				.AddColumn("Текущая оплата, р.")
					.AddNumericRenderer(node => node.CurrentPayment).Editing().Digits(2)
					.Adjustment(new Adjustment(0, 0, 10000000, 1, 10, 10))
					.AddSetter((node, cell) => ViewModel.CurrentPaymentChangedByUser(cell))
				.AddColumn("Статус оплаты")
					.AddEnumRenderer(node => node.OrderPaymentStatus)
				.AddColumn("Рассчитать остаток?")
					.AddToggleRenderer(node => node.Calculate)
					.ToggledEvent(UseFine_Toggled)
				.AddColumn("")
				.Finish();

			ytreeviewOrdersAllocate.ItemsDataSource = ViewModel.ListNodes;
			ytreeviewOrdersAllocate.ButtonReleaseEvent += YtreeviewOrdersAllocate_ButtonReleaseEvent;

			yTreeViewAllocatedOrders.ColumnsConfig = FluentColumnsConfig<ManualPaymentMatchingViewModelAllocatedNode>.Create()
				.AddColumn("№ заказа")
					.AddNumericRenderer(node => node.OrderId)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Дата заказа")
					.AddTextRenderer(node => node.OrderDate.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Сумма заказа, р.")
					.AddNumericRenderer(node => node.OrderSum)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Полная сумма оплаты\n(в т.ч. с др платежей), р.")
					.AddNumericRenderer(node => node.AllAllocatedSum)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Распределенная сумма\n(с этого платежа), р.")
					.AddNumericRenderer(node => node.AllocatedSum)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Статус оплаты")
					.AddEnumRenderer(node => node.OrderPaymentStatus)
					.XAlign(0.5f)
				.AddColumn("Статус распределения")
					.AddEnumRenderer(node => node.PaymentItemStatus)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			yTreeViewAllocatedOrders.ItemsDataSource = ViewModel.ListAllocatedNodes;
			yTreeViewAllocatedOrders.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanRevertPayFromOrderPermission, w => w.Sensitive)
				.AddBinding(vm => vm.SelectedAllocatedNode, w => w.SelectedRow)
				.InitializeFromSource();
		}

		#region Переключение вкладок

		private void RadioBtnAllocateOrdersOnToggled(object sender, EventArgs e)
		{
			if(radioBtnAllocateOrders.Active)
				notebook1.CurrentPage = 0;
		}

		private void RadioBtnAllocatedOrdersOnToggled(object sender, EventArgs e)
		{
			if(radioBtnAllocatedOrders.Active)
				notebook1.CurrentPage = 1;
		}

		#endregion

		private void UseFine_Toggled(object o, ToggledArgs args) =>
			//Вызываем через Gtk.Application.Invoke чтобы событие вызывалось уже после того как поле обновилось.
			Gtk.Application.Invoke((sender, eventArgs) => OnToggleClicked(this, EventArgs.Empty));

		private void OnToggleClicked(object sender, EventArgs e)
		{
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
				return;

			var node = selectedObj as ManualPaymentMatchingViewModelNode;

			if(node.Calculate)
				ViewModel.Calculate(node);
			else
				ViewModel.ReCalculate(node);
		}

		private void YtreeviewOrdersAllocate_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3)
				ConfigureMenu();
		}

		private void ConfigureMenu()
		{
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
				return;

			var order = ViewModel.UoW.GetById<Order>((selectedObj as ManualPaymentMatchingViewModelNode).Id);

			var menu = new Menu();

			var openOrder = new MenuItem($"Открыть заказ №{order.Id}");
			openOrder.Activated += (s, args) => ViewModel.OpenOrderCommand.Execute(order);
			openOrder.Visible = true;
			menu.Add(openOrder);

			menu.ShowAll();
			menu.Popup();
		}
	}
}
