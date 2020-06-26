using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.Dialogs.Email;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;

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
			//ylabelOrderNum.Binding.AddBinding(ViewModel, vm => vm.Entity.Id, w => w.Text).InitializeFromSource();
			yentryDebtName.Binding.AddBinding(ViewModel.Entity, vm => vm.DebtName, w => w.Text).InitializeFromSource();
			ylabelOrderDate.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text).InitializeFromSource();
			ylabelOrderAuthor.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text).InitializeFromSource();

			entityviewmodelentry1.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);
			entityviewmodelentry1.Binding.AddBinding(ViewModel.Entity, vm => vm.Client, w => w.Subject).InitializeFromSource();
			entityviewmodelentry1.Binding.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive).InitializeFromSource();
			entityviewmodelentry1.Changed += ViewModel.OnEntityViewModelEntryChanged;
			entityviewmodelentry1.CanEditReference = true;
			
			var sendEmailView = new SendDocumentByEmailView(ViewModel.SendDocViewModel);
			hbox7.Add(sendEmailView);
			sendEmailView.Show();

			ViewModel.OpenCounterpatyJournal += entityviewmodelentry1.OpenSelectDialog;
		}
		
		public override void Destroy()
		{
			entityviewmodelentry1.Changed -= ViewModel.OnEntityViewModelEntryChanged;
			ViewModel.OpenCounterpatyJournal -= entityviewmodelentry1.OpenSelectDialog;
			
			ViewModel?.Dispose();
			base.Destroy();
		}
	}
}
