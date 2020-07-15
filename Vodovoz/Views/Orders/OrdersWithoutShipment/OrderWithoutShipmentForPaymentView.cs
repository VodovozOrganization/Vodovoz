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
			this.Build();

			Configure();
		}

		private void Configure()
		{
			btnCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			ylabelOrderNum.Binding.AddBinding(ViewModel.Entity, vm => vm.Id, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			ylabelOrderDate.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text).InitializeFromSource();
			ylabelOrderAuthor.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text).InitializeFromSource();

			entityviewmodelentry1.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);

			entityviewmodelentry1.Changed += ViewModel.OnEntityViewModelEntryChanged;

			entityviewmodelentry1.Binding.AddBinding(ViewModel.Entity, vm => vm.Client, w => w.Subject).InitializeFromSource();
			entityviewmodelentry1.Binding.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive).InitializeFromSource();
			entityviewmodelentry1.CanEditReference = true;

			var sendEmailView = new SendDocumentByEmailView(ViewModel.SendDocViewModel);
			hboxSendDocuments.Add(sendEmailView);
			sendEmailView.Show();

			ViewModel.OpenCounterpartyJournal += entityviewmodelentry1.OpenSelectDialog;

			daterangepickerOrdersDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepickerOrdersDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			daterangepickerOrdersDate.PeriodChangedByUser += (s, e) => ViewModel.UpdateNodes(this, EventArgs.Empty);

			ytreeviewOrders.ColumnsConfig = FluentColumnsConfig<OrderWithoutShipmentForPaymentNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(node => node.IsSelected).ToggledEvent(UseFine_Toggled)
				.AddColumn("Номер").AddTextRenderer(node => node.OrderId.ToString())
				.AddColumn("Дата").AddTextRenderer(node => node.OrderDate.ToShortDateString())
				.AddColumn("Статус").AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Бутыли").AddTextRenderer(node => node.Bottles.ToString())
				.AddColumn("Сумма").AddTextRenderer(node => node.OrderSum.ToString())
				.AddColumn("Адрес").AddTextRenderer(node => node.DeliveryAddress)
				.Finish();

			ytreeviewOrders.ItemsDataSource = ViewModel.ObservableNodes;
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
			entityviewmodelentry1.Changed -= ViewModel.OnEntityViewModelEntryChanged;

			base.Destroy();
		}
	}
}
