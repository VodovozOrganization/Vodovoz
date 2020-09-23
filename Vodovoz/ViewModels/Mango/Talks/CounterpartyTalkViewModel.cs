using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QSReport;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.Views.Mango;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public partial class CounterpartyTalkViewModel : TalkViewModelBase
	{
		private ITdiCompatibilityNavigation tdiNavigation;
		private readonly IInteractiveQuestion interactive;
		private readonly RouteListRepository routedListRepository;
		private IUnitOfWork UoW;
		private List<CounterpartyOrderViewModel> counterpartyOrdersModels = new List<CounterpartyOrderViewModel>();
		public List<CounterpartyOrderViewModel> CounterpartyOrdersModels {
			get => counterpartyOrdersModels;
			private set {
				counterpartyOrdersModels = value;
			}
		}


		public Counterparty currentCounterparty { get;private set; }
		public event System.Action CounterpartyOrdersModelsUpdateEvent = () => { };

		public CounterpartyTalkViewModel(IEnumerable<Counterparty> clients,
			INavigationManager navigation,
			ITdiCompatibilityNavigation tdinavigation,
			IInteractiveQuestion interactive,
			IUnitOfWorkFactory unitOfWorkFactory,
			RouteListRepository routedListRepository,
			MangoManager manager) : base(navigation, manager)
		{
			this.NavigationManager = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.tdiNavigation = tdinavigation ?? throw new ArgumentNullException(nameof(navigation));

			this.interactive = interactive;
			this.routedListRepository = routedListRepository;
			UoW = unitOfWorkFactory.CreateWithoutRoot();

			if(clients != null) 
			{
				foreach(Counterparty client in clients) 
				{
					CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, unitOfWorkFactory, tdinavigation,routedListRepository);
					CounterpartyOrdersModels.Add(model);
				}
				currentCounterparty = CounterpartyOrdersModels.First().Client;
			} else
				throw new ArgumentNullException(nameof(clients));

		}

		public string GetPhoneNumber()
		{
			return "+7"+MangoManager.CallerNumber;
		}

		public IDictionary<string, CounterpartyOrderView> GetCounterpartyViewModels()
		{
			return null;
		}
		#region Взаимодействие с Mangos

		#endregion

		#region Действия View

		public void UpadateCurrentCounterparty(Counterparty counterparty)
		{
			currentCounterparty = counterparty;

		}
		public void NewClientCommand()
		{
			var page = tdiNavigation.OpenTdiTab<CounterpartyDlg>(this);
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
				Phone phone = new Phone() { Number = MangoManager.CallerNumber };
				client.Phones.Add(phone);
				clients.Add(client);
				UoW.Save<Counterparty>(client);
				CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, UnitOfWorkFactory.GetDefaultFactory, tdiNavigation, routedListRepository);
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
				Phone phone = new Phone() { Number = MangoManager.CallerNumber };
				firstClient.Phones.Add(phone);
				UoW.Save<Counterparty>(firstClient);
				UoW.Commit();
				CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(firstClient, UnitOfWorkFactory.GetDefaultFactory,tdiNavigation, routedListRepository);
				counterpartyOrdersModels.Add(model);
				currentCounterparty = firstClient;
				CounterpartyOrdersModelsUpdateEvent();

			}
			(sender as CounterpartyJournalViewModel).OnEntitySelectedResult -= ExistingCounterparty_PageClosed;
		}

		public void NewOrderCommand()
		{
			var model = CounterpartyOrdersModels.Find(m => m.Client.Id == currentCounterparty.Id);
			IPage page = tdiNavigation.OpenTdiTab<OrderDlg, Counterparty>(null, currentCounterparty);
			page.PageClosed += (sender, e) => { model.RefreshOrders(); };
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
			ReportViewDlg dialog = tdiNavigation.OpenTdiTab<ReportViewDlg, IParametersWidget>(null, parameters) as ReportViewDlg;
			parameters.OnUpdate(true);
			
		}

		public void StockBalanceCommand()
		{
			NomenclatureStockFilterViewModel filter = new NomenclatureStockFilterViewModel(
		new WarehouseRepository()
		);
			NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel, NomenclatureStockFilterViewModel>(null, filter);

		}

		public void CostAndDeliveryIntervalCommand(DeliveryPoint point)
		{
			tdiNavigation.OpenTdiTab<DeliveryPriceDlg, DeliveryPoint>(null, point);
		}

		#endregion
	}
}
