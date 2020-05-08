using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels;
using Gtk;
using Vodovoz.Domain.Orders;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Client;
using Vodovoz.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Infrastructure.Converters;
using QS.Project.Search.GtkUI;
using QS.Project.Search;
using QS.Navigation;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ManualPaymentMatchingView : TabViewBase<ManualPaymentMatchingVM>
	{
		readonly IUnitOfWork UoW;

		public ManualPaymentMatchingView(ManualPaymentMatchingVM manualPaymentLoaderVM) : base(manualPaymentLoaderVM)
		{
			this.Build();
			ViewModel.TabName = "Ручное распределение платежей";
			UoW = ViewModel.UoW;

			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			btnSave.Clicked += OnBtnSave_Clicked;
			btnCancel.Clicked += OnBtnCancel_Clicked;
			buttonComplete.Clicked += (sender, e) => ViewModel.CompleteAllocation.Execute();
			button1.Clicked += (sender, e) => ViewModel.AddCounterpatyCommand.Execute(ViewModel.Entity);

			daterangepicker1.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepicker1.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			daterangepicker1.PeriodChangedByUser += (sender, e) => UpdateNodes(this, EventArgs.Empty);
			yenumcomboOrderStatus.ItemsEnum = typeof(OrderStatus);
			yenumcomboOrderStatus.Binding.AddBinding(ViewModel, vm => vm.OrderStatusVM, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboOrderStatus.ChangedByUser += (sender, e) => UpdateNodes(this, EventArgs.Empty);
			yenumcomboOrderPaymentStatus.ItemsEnum = typeof(OrderPaymentStatus);
			yenumcomboOrderPaymentStatus.Binding.AddBinding(ViewModel, vm => vm.OrderPaymentStatusVM, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboOrderPaymentStatus.ChangedByUser += (sender, e) => UpdateNodes(this, EventArgs.Empty);

			labelTotalSum.Text = ViewModel.Entity.Total.ToString();
			labelLastBalance.Text = ViewModel.LastBalance.ToString();
			labelToAllocate.Text = ViewModel.SumToAllocate.ToString();

			ylabelCurBalance.Binding.AddBinding(ViewModel, vm => vm.CurrentBalance, v => v.Text, new DecimalToStringConverter()).InitializeFromSource();
			ylabelAllocated.Binding.AddBinding(ViewModel, vm => vm.AllocatedSum, v => v.Text, new DecimalToStringConverter()).InitializeFromSource();

			labelPayer.Text = ViewModel.Entity.CounterpartyName;
			labelPaymentNum.Text = ViewModel.Entity.PaymentNum.ToString();
			labelDate.Text = ViewModel.Entity.Date.ToShortDateString();

			//var text = ViewModel.Entity.PaymentPurpose + ViewModel.Entity.PaymentPurpose + ViewModel.Entity.PaymentPurpose;
			//ytextviewPaymentPurpose.Binding.AddBinding(ViewModel.Entity, vm => vm.PaymentPurpose, v => v.Buffer.Text).InitializeFromSource();
			ytextviewPaymentPurpose.Buffer.Text = ViewModel.Entity.PaymentPurpose;
			ytextviewComments.Binding.AddBinding(ViewModel.Entity, vm => vm.Comment, v => v.Buffer.Text).InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices));

			entryCounterparty.Binding.AddBinding(ViewModel.Entity, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();

			var searchView = new SearchView((SearchViewModel)ViewModel.Search);
			hboxSearch.Add(searchView);
			searchView.Show();

			ViewModel.Search.OnSearch += UpdateNodes;

			ytreeviewOrdersAllocate.ColumnsConfig = FluentColumnsConfig<ManualPaymentMatchingVMNode>.Create()
				.AddColumn("№ заказа").AddTextRenderer(node => node.Id.ToString()).XAlign(0.5f)
				.AddColumn("Статус").AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Дата заказа").AddTextRenderer(node => node.OrderDate.ToShortDateString()).XAlign(0.5f)
				.AddColumn("Сумма заказа, р.").AddTextRenderer(node => node.ActualOrderSum.ToString()).XAlign(0.5f)
				.AddColumn("Прошлые оплаты, р.").AddNumericRenderer(node => node.LastPayments).Digits(2)
				.AddColumn("Текущая оплата, р.").AddNumericRenderer(node => node.CurrentPayment).Editing().Digits(2)
					.Adjustment(new Adjustment(0, 0, 10000000, 1, 10, 10)).EditedEvent(TreeViewCurentPaymentEdited)
				.AddColumn("Рассчитать остаток?").AddToggleRenderer(node => node.Calculate).ToggledEvent(UseFine_Toggled)
				.AddColumn("")
			.Finish();

			ytreeviewOrdersAllocate.ButtonReleaseEvent += YtreeviewOrdersAllocate_ButtonReleaseEvent;

			UpdateNodes(this, EventArgs.Empty);
		}

		void TreeViewCurentPaymentEdited(object o, EditedArgs args) 
			=> Application.Invoke(delegate { CurrentPaymentChangedByUser(this, EventArgs.Empty); });

		void CurrentPaymentChangedByUser(object o, EventArgs args) 
		{ 
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
				return;

			var node = selectedObj as ManualPaymentMatchingVMNode;

			ViewModel.CurrentPaymentChangedByUser(node);
		}

		void UseFine_Toggled(object o, ToggledArgs args) =>
			//Вызываем через Application.Invoke чтобы событие вызывалось уже после того как поле обновилось.
			Application.Invoke(delegate { OnToggleClicked(this, EventArgs.Empty); });

		void OnToggleClicked(object sender, EventArgs e)
		{
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
				return;

			var node = selectedObj as ManualPaymentMatchingVMNode;

			if(node.Calculate)
				ViewModel.Calculate(node);
			else
				ViewModel.ReCalculate(node);
		}

		void YtreeviewOrdersAllocate_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3)
				ConfigureMenu();
		}

		void ConfigureMenu()
		{
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
				return;

			var order = UoW.GetById<Order>((selectedObj as ManualPaymentMatchingVMNode).Id);

			var menu = new Menu();

			var openOrder = new MenuItem($"Открыть заказ №{order.Id}");
			openOrder.Activated += (s, args) => ViewModel.OpenOrderCommand.Execute(order);
			openOrder.Visible = true;
			menu.Add(openOrder);

			menu.ShowAll();
			menu.Popup();
		}


		void OnBtnSave_Clicked(object sender, System.EventArgs e)
		{
			ViewModel.SaveViewModel();
		}

		void OnBtnCancel_Clicked(object sender, System.EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}

		void UpdateNodes(object sender, EventArgs e)
		{
			ViewModel.ClearProperties();

			ytreeviewOrdersAllocate.ItemsDataSource = ViewModel.UpdateNodes();
		}
	}
}
