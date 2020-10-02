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
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.JournalNodes;
using Vodovoz.JournalSelector;
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
		private IEntityAutocompleteSelectorFactory counterpartySelectorFactory;

		public List<CounterpartyOrderViewModel> CounterpartyOrdersModels {
			get => counterpartyOrdersModels;
			private set {
				counterpartyOrdersModels = value;
			}
		}

		public Counterparty currentCounterparty { get;private set; }
		public event System.Action CounterpartyOrdersModelsUpdateEvent = () => { };

		public CounterpartyTalkViewModel(IEnumerable<int> clientsIds,
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

			if(clientsIds.Count() > 0) 
			{
				var clients = UoW.GetById<Counterparty>(clientsIds);
				foreach(Counterparty client in clients)
				{
					CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, unitOfWorkFactory, tdinavigation,routedListRepository,this.MangoManager);
					CounterpartyOrdersModels.Add(model);
				}
				currentCounterparty = CounterpartyOrdersModels.First().Client;
			} else
				throw new ArgumentNullException(nameof(clientsIds));
		}

		public string GetPhoneNumber()
		{
			return "+7" + Phone.Number;
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
				client.Phones.Add(Phone);
				clients.Add(client);
				UoW.Save<Counterparty>(client);
				CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, UnitOfWorkFactory.GetDefaultFactory, tdiNavigation, routedListRepository,this.MangoManager);
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
			if(counterpartyOrdersModels.FirstOrDefault(c => c.Client.Id == firstClient.Id) == null) {
				if(interactive.Question($"Доабать телефон к контагенту {firstClient.Name} ?", "Телефон контрагента")) {
					firstClient.Phones.Add(Phone);
					UoW.Save<Counterparty>(firstClient);
					UoW.Commit();
					CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(firstClient, UnitOfWorkFactory.GetDefaultFactory, tdiNavigation, routedListRepository,this.MangoManager);
					counterpartyOrdersModels.Add(model);
					currentCounterparty = firstClient;
					CounterpartyOrdersModelsUpdateEvent();

				}
			} else return;
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
			var nomenclatureRepository = new NomenclatureRepository();

			IEntityAutocompleteSelectorFactory employeeSelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(
					ServicesConfig.CommonServices);

			IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
					CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig
					.CommonServices, new NomenclatureFilterViewModel(), counterpartySelectorFactory,
					nomenclatureRepository, UserSingletonRepository.GetInstance());

			ISubdivisionRepository subdivisionRepository = new SubdivisionRepository();

			var parameters = new Dictionary<string, object> {
				{"client", currentCounterparty},
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{ "unitOfWorkFactory",UnitOfWorkFactory.GetDefaultFactory },
				//Autofac: IEmployeeService 
				{"employeeSelectorFactory", employeeSelectorFactory},
				{"counterpartySelectorFactory", counterpartySelectorFactory},
				{"subdivisionService",subdivisionRepository},
				//Autofac: ICommonServices
				{"nomenclatureSelectorFactory" , nomenclatureSelectorFactory},
				{"nomenclatureRepository",nomenclatureRepository},
				//Autofac: IUserRepository
				{"phone", "+7" +MangoManager.Phone.Number }
			};
			tdiNavigation.OpenTdiTabOnTdiNamedArgs<CreateComplaintViewModel>(null,parameters);
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
