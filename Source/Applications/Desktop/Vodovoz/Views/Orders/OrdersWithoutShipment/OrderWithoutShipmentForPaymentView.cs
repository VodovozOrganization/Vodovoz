using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Dialogs.Email;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;

namespace Vodovoz.Views.Orders.OrdersWithoutShipment
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderWithoutShipmentForPaymentView : TabViewBase<OrderWithoutShipmentForPaymentViewModel>
	{
		public OrderWithoutShipmentForPaymentView(OrderWithoutShipmentForPaymentViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			ybtnOpenBill.Clicked += (sender, e) => ViewModel.OpenBillCommand.Execute();
			
			ylabelOrderNum.Binding.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			ylabelOrderDate.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text).InitializeFromSource();
			ylabelOrderAuthor.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text).InitializeFromSource();
			yCheckBtnHideSignature.Binding.AddBinding(ViewModel.Entity, e => e.HideSignature, w => w.Active).InitializeFromSource();

			entityViewModelEntryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory());

			entityViewModelEntryCounterparty.Changed += ViewModel.OnEntityViewModelEntryChanged;

			entityViewModelEntryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Client, w => w.Subject).InitializeFromSource();
			entityViewModelEntryCounterparty.Binding.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive).InitializeFromSource();
			entityViewModelEntryCounterparty.CanEditReference = true;

			var sendEmailView = new SendDocumentByEmailView(ViewModel.SendDocViewModel);
			hboxSendDocuments.Add(sendEmailView);
			sendEmailView.Show();

			ViewModel.OpenCounterpartyJournal += entityViewModelEntryCounterparty.OpenSelectDialog;

			daterangepickerOrdersDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepickerOrdersDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			daterangepickerOrdersDate.PeriodChangedByUser += UpdateAvailableOrders;

			ytreeviewOrders.ColumnsConfig = FluentColumnsConfig<OrderWithoutShipmentForPaymentNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(node => node.IsSelected).ToggledEvent(UseFine_Toggled)
				.AddColumn("Номер").AddTextRenderer(node => node.OrderId.ToString())
				.AddColumn("Дата\nдоставки").AddTextRenderer(node => node.OrderDate.ToShortDateString())
				.AddColumn("Статус").AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Бутыли").AddTextRenderer(node => $"{node.Bottles:N0}")
				.AddColumn("Сумма").AddTextRenderer(node => node.OrderSum.ToString())
				.AddColumn("Адрес").AddTextRenderer(node => node.DeliveryAddress)
				.Finish();

			ytreeviewOrders.ItemsDataSource = ViewModel.ObservableAvailableOrders;
		}

		private void UpdateAvailableOrders(object sender, EventArgs e)
		{
			ViewModel.UpdateAvailableOrders();
		}

		void UseFine_Toggled(object o, ToggledArgs args) =>
			//Вызываем через Application.Invoke чтобы событие вызывалось уже после того как поле обновилось.
			Application.Invoke(delegate { OnToggleClicked(this, EventArgs.Empty); });

		void OnToggleClicked(object sender, EventArgs e)
		{
			var selectedObj = ytreeviewOrders.GetSelectedObject();

			if(selectedObj == null)
				return;

			ViewModel.SelectedNode = selectedObj as OrderWithoutShipmentForPaymentNode;

			ViewModel.UpdateItems();
		}

		public override void Destroy()
		{
			entityViewModelEntryCounterparty.Changed -= ViewModel.OnEntityViewModelEntryChanged;
			ViewModel.OpenCounterpartyJournal -= entityViewModelEntryCounterparty.OpenSelectDialog;
			base.Destroy();
		}
	}
}
