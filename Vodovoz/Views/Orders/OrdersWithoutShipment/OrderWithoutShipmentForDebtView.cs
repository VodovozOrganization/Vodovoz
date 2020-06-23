using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.Dialogs.Email;
using Vodovoz.Repositories.HumanResources;
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
			ybtnSendEmail.Clicked += (sender, e) => ViewModel.SendEmailCommand.Execute();

			//ylabelOrderNum.Binding.AddBinding(ViewModel, vm => vm.Entity.Id, w => w.Text).InitializeFromSource();
			ylabelOrderDate.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text).InitializeFromSource();
			ylabelOrderAuthor.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text).InitializeFromSource();

			//yentryEmail.Binding.AddBinding();

			entityviewmodelentry1.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);
			entityviewmodelentry1.Binding.AddBinding(ViewModel.Entity, vm => vm.Client, w => w.Subject).InitializeFromSource();
			entityviewmodelentry1.Binding.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive).InitializeFromSource();
			entityviewmodelentry1.Changed += ViewModel.OnEntityViewModelEntryChanged;
			entityviewmodelentry1.CanEditReference = true;

			var sendDocumentByEmailViewModel = new SendDocumentByEmailViewModel(new EmailRepository(), EmployeeSingletonRepository.GetInstance(), ViewModel.UoW);
			ViewModel.SendDocViewModel = sendDocumentByEmailViewModel;
			var sendEmailView = new SendDocumentByEmailView(sendDocumentByEmailViewModel);
			hbox7.Add(sendEmailView);
			sendEmailView.Show();

			ViewModel.OpenCounterpatyJournal += entityviewmodelentry1.OpenSelectDialog;
		}
	}
}
