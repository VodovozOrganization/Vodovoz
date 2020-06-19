using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;

namespace Vodovoz.Views.Orders.OrdersWithoutShipment
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderWithoutShipmentForDebtView : TabViewBase<OrderWithoutShipmentForDebtViewModel>
	{
		public OrderWithoutShipmentForDebtView(OrderWithoutShipmentForDebtViewModel viewModel) : base(viewModel)
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
			entityviewmodelentry1.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Client, w => w.Subject).InitializeFromSource();
			entityviewmodelentry1.CanEditReference = true;

			ViewModel.OpenCounterpatyJournal += entityviewmodelentry1.OpenSelectDialog;
		}
	}
}
