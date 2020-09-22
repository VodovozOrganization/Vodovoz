using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels;
using Gtk;
using Vodovoz.Domain.Orders;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using System;
using Vodovoz.Infrastructure.Converters;
using QS.Project.Search.GtkUI;
using QS.Project.Search;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ManualPaymentMatchingView : TabViewBase<ManualPaymentMatchingViewModel>
	{
		public ManualPaymentMatchingView(ManualPaymentMatchingViewModel manualPaymentLoaderViewModel) : base(manualPaymentLoaderViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg() {
			notebook1.ShowTabs = false;

			#region Radio buttons

			radioBtnAllocateOrders.Active = true;
			radioBtnAllocateOrders.Toggled += RadioBtnAllocateOrdersOnToggled;
			radioBtnAllocatedOrders.Toggled += RadioBtnAllocatedOrdersOnToggled;
			radioBtnAllocatedOrders.Binding.AddBinding(ViewModel, vm => vm.HasPaymentItems, w => w.Sensitive).InitializeFromSource();

			#endregion
			
			btnSave.Clicked += (sender, args) => ViewModel.SaveViewModelCommand.Execute();
			btnCancel.Clicked += (sender, args) => ViewModel.CloseViewModelCommand.Execute();
			buttonComplete.Clicked += (sender, args) => ViewModel.CompleteAllocation.Execute();
			btnAddCounterparty.Clicked += (sender, args) => ViewModel.AddCounterpatyCommand.Execute(ViewModel.Entity);
			ybtnRevertPayment.Clicked += (sender, args) => ViewModel.RevertAllocatedSum.Execute();
			ybtnRevertPayment.Binding.AddBinding(ViewModel, vm => vm.CanRevertPayFromOrder, w => w.Sensitive).InitializeFromSource();

			daterangepicker1.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepicker1.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			daterangepicker1.PeriodChangedByUser += (sender, e) => ViewModel.UpdateNodes();
			yenumcomboOrderStatus.ItemsEnum = typeof(OrderStatus);
			yenumcomboOrderStatus.Binding.AddBinding(ViewModel, vm => vm.OrderStatusVM, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboOrderStatus.ChangedByUser += (sender, e) => ViewModel.UpdateNodes();
			yenumcomboOrderPaymentStatus.ItemsEnum = typeof(OrderPaymentStatus);
			yenumcomboOrderPaymentStatus.Binding.AddBinding(ViewModel, vm => vm.OrderPaymentStatusVM, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboOrderPaymentStatus.ChangedByUser += (sender, e) => ViewModel.UpdateNodes();

			labelTotalSum.Text = ViewModel.Entity.Total.ToString();
			labelLastBalance.Binding.AddBinding(ViewModel, vm => vm.LastBalance, w => w.Text, new DecimalToStringConverter()).InitializeFromSource();
			labelToAllocate.Binding.AddBinding(ViewModel, vm => vm.SumToAllocate, w => w.Text, new DecimalToStringConverter()).InitializeFromSource();

			ylabelCurBalance.Binding.AddBinding(ViewModel, vm => vm.CurrentBalance, v => v.Text, new DecimalToStringConverter()).InitializeFromSource();
			ylabelAllocated.Binding.AddBinding(ViewModel, vm => vm.AllocatedSum, v => v.Text, new DecimalToStringConverter()).InitializeFromSource();
			ylabelCounterpartyDebt.Binding.AddBinding(ViewModel, vm => vm.CounterpartyDebt, v => v.Text, new DecimalToStringConverter()).InitializeFromSource();

			labelPayer.Text = ViewModel.Entity.CounterpartyName;
			labelPaymentNum.Text = ViewModel.Entity.PaymentNum.ToString();
			labelDate.Text = ViewModel.Entity.Date.ToShortDateString();
			
			ytextviewPaymentPurpose.Buffer.Text = ViewModel.Entity.PaymentPurpose;
			ytextviewComments.Binding.AddBinding(ViewModel.Entity, vm => vm.Comment, v => v.Buffer.Text).InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices));

			entryCounterparty.Binding.AddBinding(ViewModel.Entity, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();
			entryCounterparty.ChangedByUser += (sender, e) =>
			{
				ViewModel.UpdateNodes();
				ViewModel.GetLastBalance();
				ViewModel.GetCounterpatyDebt();
			};

			var searchView = new SearchView((SearchViewModel)ViewModel.Search);
			hboxSearch.Add(searchView);
			searchView.Show();

			ytreeviewOrdersAllocate.ColumnsConfig = FluentColumnsConfig<ManualPaymentMatchingViewModelNode>.Create()
				.AddColumn("№ заказа").AddTextRenderer(node => node.Id.ToString()).XAlign(0.5f)
				.AddColumn("Статус").AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Дата заказа").AddTextRenderer(node => node.OrderDate.ToShortDateString()).XAlign(0.5f)
				.AddColumn("Сумма заказа, р.").AddTextRenderer(node => node.ActualOrderSum.ToString()).XAlign(0.5f)
				.AddColumn("Прошлые оплаты, р.").AddNumericRenderer(node => node.LastPayments).Digits(2)
				.AddColumn("Текущая оплата, р.").AddNumericRenderer(node => node.CurrentPayment).Editing().Digits(2)
					.Adjustment(new Adjustment(0, 0, 10000000, 1, 10, 10)).EditedEvent(TreeViewCurentPaymentEdited)
				.AddColumn("Статус оплаты").AddEnumRenderer(node => node.OrderPaymentStatus)
				.AddColumn("Рассчитать остаток?").AddToggleRenderer(node => node.Calculate).ToggledEvent(UseFine_Toggled)
				.AddColumn("")
			.Finish();
			
			ytreeviewOrdersAllocate.ItemsDataSource = ViewModel.ListNodes;
			ytreeviewOrdersAllocate.ButtonReleaseEvent += YtreeviewOrdersAllocate_ButtonReleaseEvent;
			
			yTreeViewAllocatedOrders.ColumnsConfig = FluentColumnsConfig<ManualPaymentMatchingViewModelAllocatedNode>.Create()
				.AddColumn("№ заказа")
					.AddTextRenderer(node => node.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Дата заказа")
					.AddTextRenderer(node => node.OrderDate.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Сумма заказа, р.")
					.AddTextRenderer(node => node.ActualOrderSum.ToString())
					.XAlign(0.5f)
				.AddColumn("Прошлая оплата, р.")
					.AddNumericRenderer(node => node.LastPayments)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Распределенная сумма, р.").AddNumericRenderer(node => node.AllocatedSum)
					.Editing()
					.Digits(2)
					.XAlign(0.5f)
					.Adjustment(new Adjustment(0, 0, 10000000, 1, 10, 10))
					.EditedEvent(TreeViewAllocatedSumEdited)
				.AddColumn("Статус оплаты")
					.AddEnumRenderer(node => node.OrderPaymentStatus)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			yTreeViewAllocatedOrders.ItemsDataSource = ViewModel.ListAllocatedNodes;
			yTreeViewAllocatedOrders.Binding.AddBinding(ViewModel, vm => vm.CanRevertPayFromOrder, w => w.Sensitive).InitializeFromSource();
			//UpdateNodes(this, EventArgs.Empty);
		}

		#region Переключение вкладок

		private void RadioBtnAllocateOrdersOnToggled(object sender, EventArgs e) {
			if (radioBtnAllocateOrders.Active)
				notebook1.CurrentPage = 0;
		}
		
		private void RadioBtnAllocatedOrdersOnToggled(object sender, EventArgs e) {
			if (radioBtnAllocatedOrders.Active)
				notebook1.CurrentPage = 1;
		}

		#endregion
		
		private void TreeViewCurentPaymentEdited(object o, EditedArgs args) 
			=> Application.Invoke((sender, eventArgs) => CurrentPaymentChangedByUser(this, EventArgs.Empty));

		private void TreeViewAllocatedSumEdited(object o, EditedArgs args)
			=> Application.Invoke((sender, eventArgs) => TreeViewAllocatedSumChangedByUser(this, args));
			
		private void CurrentPaymentChangedByUser(object o, EventArgs args) 
		{ 
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
				return;

			var node = selectedObj as ManualPaymentMatchingViewModelNode;

			ViewModel.CurrentPaymentChangedByUser(node);
		}
		
		private void TreeViewAllocatedSumChangedByUser(object o, EditedArgs args) 
		{ 
			var selectedObj = yTreeViewAllocatedOrders.GetSelectedObject();

			if(selectedObj == null)
				return;

			var node = selectedObj as ManualPaymentMatchingViewModelAllocatedNode;
			
			ViewModel.TreeViewAllocatedSumChangedByUser(node);
		}
		
		private void UseFine_Toggled(object o, ToggledArgs args) =>
			//Вызываем через Application.Invoke чтобы событие вызывалось уже после того как поле обновилось.
			Application.Invoke((sender, eventArgs) => OnToggleClicked(this, EventArgs.Empty));

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
