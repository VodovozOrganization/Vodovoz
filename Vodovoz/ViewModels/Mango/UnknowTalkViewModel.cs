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
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Mango
{
	public class UnknowTalkViewModel : WindowDialogViewModelBase
	{
		private readonly ITdiCompatibilityNavigation tdiNavigation;
		private readonly IInteractiveQuestion interactive;
		private IUnitOfWork UoW;
		private Phone phone;
		public Phone Phone {
			get => phone;
			private set { phone = value; }
		}
		public UnknowTalkViewModel(Phone phone, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiCompatibilityNavigation navigation, 
			IInteractiveQuestion interactive) : base(navigation)
		{
			this.tdiNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			Title = "Входящий новый номер";
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;

			this.phone = phone;
		}

		#region Действия View

		public void SelectNewConterparty()
		{
			var page = tdiNavigation.OpenTdiTab<CounterpartyDlg>(null);
			var tab = page.TdiTab as CounterpartyDlg;
			tab.Entity.Phones.First().Number = "+7-000-000-00-00"; //FIXME
			page.PageClosed += NewCounerpatry_PageClosed;
		}

		public void SelectExistConterparty()
		{
			var page = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			page.ViewModel.OnEntitySelectedResult += ExistingCounterparty_PageClosed;
		}

		void NewCounerpatry_PageClosed(object sender, PageClosedEventArgs e)
		{ 
			if(e.CloseSource == CloseSource.Save) {
				List<Counterparty> clients = new List<Counterparty>();
				Counterparty client = ((sender as TdiTabPage).TdiTab as CounterpartyDlg).Counterparty;
				client.Phones.Add(phone);
				clients.Add(client);
				UoW.Save<Counterparty>(client);
				NavigationManager.OpenViewModel<CounterpartyTalkViewModel, IEnumerable<Counterparty>,Phone>(null,clients,phone);
				this.Close(false, CloseSource.Self);
			}
		}

		void ExistingCounterparty_PageClosed(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var counterpartyNode = e.SelectedNodes.First() as CounterpartyJournalNode;
			IEnumerable<Counterparty> clients = UoW.Session.Query<Counterparty>().Where(c => c.Id == counterpartyNode.Id);
			Counterparty firstClient = clients.First();
			if(interactive.Question($"Доабать телефон к контагенту {firstClient.Name} ?", "Телефон контрагента")) {
				firstClient.Phones.Add(phone);
				UoW.Save<Counterparty>(firstClient);
				UoW.Commit();
				NavigationManager.OpenViewModel<CounterpartyTalkViewModel, IEnumerable<Counterparty>, Phone>(null, clients, phone);
				this.Close(false, CloseSource.Self);
			}
		}

		public void CreateComplaintCommand()
		{
			var parameters = new Dictionary<string, object> {
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{"employeeSelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices)},
				{"counterpartySelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices)},
				{"phone", "не реализовано"}//FIXME
			};
			tdiNavigation.OpenTdiTabNamedArgs<CreateComplaintViewModel>(null, parameters);
		}

		public void StockBalanceCommand()
		{
			NomenclatureStockFilterViewModel filter = new NomenclatureStockFilterViewModel(
			new WarehouseRepository()
);
			NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel, NomenclatureStockFilterViewModel>(null, filter);

		}

		public void CostAndDeliveryIntervalCommand()
		{
			tdiNavigation.OpenTdiTab<DeliveryPriceDlg>(null);
		}

		#region CallEvents
		public void FinishCallCommand()
		{
			//FIXME
		}

		public void ForwardCallCommand()
		{
			//FIXME
		}

		public void ForwardToConsultationCommand()
		{
			//FIXME
		}
		#endregion

		//public void
		#endregion

	}
}
