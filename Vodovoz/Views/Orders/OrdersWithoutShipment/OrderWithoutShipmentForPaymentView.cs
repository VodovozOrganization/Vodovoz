using System;
using Gamma.ColumnConfig;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ReportsParameters;
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
			ybtnSendEmail.Clicked += (sender, e) => ViewModel.SendEmailCommand.Execute();

			//ylabelOrderNum.Binding.AddBinding(ViewModel, vm => vm.Entity.Id, w => w.Text).InitializeFromSource();
			ylabelOrderDate.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text).InitializeFromSource();
			ylabelOrderAuthor.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text).InitializeFromSource();

			//yentryEmail.Binding.AddBinding();

			entityviewmodelentry1.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);
			entityviewmodelentry1.Binding.AddBinding(ViewModel.Entity, vm => vm.Client, w => w.Subject).InitializeFromSource();
			entityviewmodelentry1.CanEditReference = true;
			entityviewmodelentry1.Changed += (s, e) => UpdateNodes(this, EventArgs.Empty);
			
			ViewModel.OpenCounterpatyJournal += entityviewmodelentry1.OpenSelectDialog;

			daterangepickerOrdersDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepickerOrdersDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			daterangepickerOrdersDate.PeriodChangedByUser += (s, e) => UpdateNodes(this, EventArgs.Empty);
			
			ytreeviewOrders.ColumnsConfig = FluentColumnsConfig<OrderWithoutShipmentForPaymentNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(node => node.IsSelected)
				.AddColumn("Номер").AddTextRenderer(node => node.OrderId.ToString())
				.AddColumn("Дата").AddTextRenderer(node => node.OrderDate.ToShortDateString())
				.AddColumn("Статус").AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Бутыли").AddTextRenderer(node => node.Bottles.ToString())
				.AddColumn("Сумма").AddTextRenderer(node => node.OrderSum.ToString())
				.AddColumn("Адрес").AddTextRenderer(node => node.DeliveryAddress)
				.Finish();

			//ytreeviewOrders.ItemsDataSource = ViewModel.UpdateNodes();
		}

		private void UpdateNodes(Object sender, EventArgs e)
		{
			ytreeviewOrders.ItemsDataSource = ViewModel.UpdateNodes();
		}
	}
}
