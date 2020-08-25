using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Mango
{
	public class IncomingCallViewModel : UowDialogViewModelBase
	{
		private readonly ITdiCompatibilityNavigation tdiCompatibilityNavigation;
		private readonly IInteractiveQuestion interactive;
		private Phone phone;
		public Phone Phone {
			get => phone;
			private set { }
		}
		public IncomingCallViewModel(Phone phone, IUnitOfWorkFactory unitOfWorkFactory, ITdiCompatibilityNavigation navigation, IInteractiveQuestion interactive) : base(unitOfWorkFactory, navigation)
		{
			this.tdiCompatibilityNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			Title = "Входящий новый номер";

			this.phone = phone;
		}

		#region Действия View

		public void CreateComplaint()
		{
			var parameters = new Dictionary<string, object> {
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{"employeeSelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices)},
				{"counterpartySelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices)},
				{"phone", "не реализовано"}//FIXME
			};
			tdiCompatibilityNavigation.OpenTdiTabNamedArgs<CreateComplaintViewModel>(null, parameters);
		}

		public void SelectNewConterparty()
		{
			var page = tdiCompatibilityNavigation.OpenTdiTab<CounterpartyDlg>(null);
			var tab = page.TdiTab as CounterpartyDlg;
			tab.Entity.Phones.First().Number = "+7-000-000-00-00"; //FIXME
			page.PageClosed += NewCounerpatry_PageClosed;
		}

		void NewCounerpatry_PageClosed(object sender, PageClosedEventArgs e)
		{
			if(e.CloseSource == CloseSource.Save) { }
		}

		public void SelectExistConterparty()
		{
			var page = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			page.ViewModel.OnEntitySelectedResult += ExistingCounterparty_PageClosed;
		}

		void ExistingCounterparty_PageClosed(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var counterpartyNode = e.SelectedNodes.First() as CounterpartyJournalNode;
			IEnumerable<Counterparty> clients = UoW.Session.Query<Counterparty>().Where(c => c.Id == counterpartyNode.Id);
			Counterparty firstClient = clients.First();
			if(interactive.Question($"Доабать телефон к контагенту {firstClient.Name} ?", "Телефон контрагента")) {
				firstClient.Phones.Add(phone);
				UoW.Save<Counterparty>(firstClient);
				NavigationManager.OpenViewModel<FullInternalCallViewModel, IEnumerable<Counterparty>, Phone>(null, clients, phone);
				this.Close(false, CloseSource.Self);
			}
		}

		#endregion

	}
}
