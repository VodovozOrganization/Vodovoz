using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using QS.Views.Dialog;
using Gtk;
using Vodovoz.Views.Mango;
using System.Collections.Generic;
using Vodovoz.Dialogs;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.Project.Services;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Representations;
using Vodovoz.Reports;
using Vodovoz.Services.Permissions;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.EntityRepositories.Store;
using QS.Project.Journal;
using QSReport;
using Vodovoz.Domain.Contacts;
using Vodovoz.Dialogs.Sale;
using Vodovoz.JournalNodes;
using QS.Dialog;

namespace Vodovoz.ViewModels.Mango
{
	public partial class CounterpartyTalkViewModel : UowDialogViewModelBase
	{
		private ITdiCompatibilityNavigation tdiNavigation;
		private readonly IInteractiveQuestion interactive;

		private List<CounterpartyOrderViewModel> counterpartyOrdersModels = new List<CounterpartyOrderViewModel>();
		public List<CounterpartyOrderViewModel> CounterpartyOrdersModels {
			get => counterpartyOrdersModels;
			private set {
				counterpartyOrdersModels = value;
			}
		}


		private Counterparty currentCounterparty { get; set; }
		private Phone phone;

		//public delegate void GotTheNewCounterpartyOrderViewModel(object sender, EventArgs e);
		public event System.Action CounterpartyOrdersModelsUpdateEvent = () => { };

		public Phone Phone {
			get => phone;
			private set { phone = value; }
		}

		public CounterpartyTalkViewModel(IEnumerable<Counterparty> clients,
			Phone phone,
			INavigationManager navigation,
			ITdiCompatibilityNavigation tdinavigation,
			IInteractiveQuestion interactive,
			IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory, navigation)
		{
			this.NavigationManager = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.tdiNavigation = tdinavigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive;
			Title = "Входящий звонок существующего контрагента";

			this.phone = phone ?? throw new ArgumentNullException(nameof(phone));
			if(clients != null) 
			{
				foreach(Counterparty client in clients) 
				{
					CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, unitOfWorkFactory, navigation, tdinavigation);
					CounterpartyOrdersModels.Add(model);
				}
				currentCounterparty = CounterpartyOrdersModels.First().Client;
			} else
				throw new ArgumentNullException(nameof(clients));

		}

		void Configure()
		{

		}


		public IDictionary<string, CounterpartyOrderView> GetCounterpartyViewModels()
		{
			return null;
		}
		#region Взаимодействие с Mangos

		//private IList<Counterparty> GetCounterpartiesByPhone(Phone phone)
		//{
		//	return IList<Counterparty>
		//}
		#endregion

		#region Действия View

		public void UpadateCurrentCounterparty(Counterparty counterparty)
		{
			currentCounterparty = counterparty;

		}
		public void NewClientCommand()
		{
			var page = tdiNavigation.OpenTdiTab<CounterpartyDlg>(this,OpenPageOptions.AsSlave);
			var tab = page.TdiTab as CounterpartyDlg;
			page.PageClosed += NewCounerpatry_PageClosed;
		}

		public void ExistingClientCommand()
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
				CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, UnitOfWorkFactory, NavigationManager, tdiNavigation);
				counterpartyOrdersModels.Add(model);
				currentCounterparty = client;
				CounterpartyOrdersModelsUpdateEvent();
			}
			(sender as IPage).PageClosed -= NewCounerpatry_PageClosed;
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
				CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(firstClient, UnitOfWorkFactory, NavigationManager, tdiNavigation);
				counterpartyOrdersModels.Add(model);
				currentCounterparty = firstClient;
				CounterpartyOrdersModelsUpdateEvent();

			}
			(sender as CounterpartyJournalViewModel).OnEntitySelectedResult -= ExistingCounterparty_PageClosed;
		}

		public void NewOrderCommand()
		{

			tdiNavigation.OpenTdiTab<OrderDlg, Counterparty>(null, currentCounterparty);
		}


		public void AddComplainCommand()
		{
			var parameters = new Dictionary<string, object> {
				{"client", currentCounterparty},
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{"employeeSelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices)},
				{"counterpartySelectorFactory", new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices)},
				{"phone", "не реализовано"}//FIXME
			};
			tdiNavigation.OpenTdiTabNamedArgs<CreateComplaintViewModel>(null,parameters);
		}

		public void BottleActCommand()
		{
			var parameters = new Vodovoz.Reports.RevisionBottlesAndDeposits();
			parameters.SetCounterparty(currentCounterparty);
			tdiNavigation.OpenTdiTab<ReportViewDlg, IParametersWidget>(null, parameters);
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

		#endregion
	}
}
