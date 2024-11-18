using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.Views.Mango;

namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	public partial class CounterpartyTalkViewModel : TalkViewModelBase, IDisposable
	{
		private readonly ITdiCompatibilityNavigation _tdiNavigation;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListRepository _routedListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IOrderSettings _orderSettings;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IUnitOfWork _uow;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly IReportInfoFactory _reportInfoFactory;
		private IPage<CounterpartyJournalViewModel> _counterpartyJournalPage;

		public List<CounterpartyOrderViewModel> CounterpartyOrdersViewModels { get; private set; } = new List<CounterpartyOrderViewModel>();

		public Counterparty currentCounterparty { get;private set; }
		public event Action CounterpartyOrdersModelsUpdateEvent = () => { };

		public CounterpartyTalkViewModel(
			ITdiCompatibilityNavigation tdinavigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListRepository routedListRepository,
			IRouteListItemRepository routeListItemRepository,
			IInteractiveService interactiveService,
			IOrderSettings orderSettings, 
			MangoManager manager,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			INomenclatureSettings nomenclatureSettings,
			IOrderRepository orderRepository,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ICallTaskWorker callTaskWorker,
			IEmployeeRepository employeeRepository,
			ICallTaskRepository callTaskRepository,
			IReportInfoFactory reportInfoFactory
			)
			: base(tdinavigation, manager)
		{
			_tdiNavigation = tdinavigation ?? throw new ArgumentNullException(nameof(tdinavigation));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_routedListRepository = routedListRepository ?? throw new ArgumentNullException(nameof(routedListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_uow = _unitOfWorkFactory.CreateWithoutRoot();
			_deliveryPointJournalFactory =
				deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_callTaskRepository = callTaskRepository ?? throw new ArgumentNullException(nameof(callTaskRepository));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			if(ActiveCall.CounterpartyIds.Any())
			{
				var clients = _uow.GetById<Counterparty>(ActiveCall.CounterpartyIds);

				foreach(Counterparty client in clients)
				{
					var model = new CounterpartyOrderViewModel(
						client,
						_unitOfWorkFactory,
						tdinavigation,
						routedListRepository,
						MangoManager,
						_orderSettings,
						_deliveryRulesSettings,
						_nomenclatureSettings,
						_callTaskWorker,
						_employeeRepository,
						_orderRepository,
						_routeListItemRepository,
						_callTaskRepository);

					CounterpartyOrdersViewModels.Add(model);
				}
				
				currentCounterparty = CounterpartyOrdersViewModels.FirstOrDefault().Client;
			} else
				throw new InvalidProgramException("Открыт диалог разговора с имеющимся контрагентом, но ни одного id контрагента не найдено.");
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
			var page = _tdiNavigation.OpenTdiTab<CounterpartyDlg, Phone>(this, ActiveCall.Phone);
			page.PageClosed += OnNewCounterpartyPageClosed;
		}

		public void ExistingClientCommand()
		{
			_counterpartyJournalPage = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			_counterpartyJournalPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			_counterpartyJournalPage.ViewModel.OnEntitySelectedResult -= OnExistingCounterpartyPageClosed;
			_counterpartyJournalPage.ViewModel.OnEntitySelectedResult += OnExistingCounterpartyPageClosed;
		}

		private void OnNewCounterpartyPageClosed(object sender, PageClosedEventArgs e)
		{
			if(e.CloseSource == CloseSource.Save)
			{
				Counterparty client = ((sender as TdiTabPage).TdiTab as CounterpartyDlg).Counterparty;
				
				var model = 
					new CounterpartyOrderViewModel(
						client,
						_unitOfWorkFactory,
						_tdiNavigation,
						_routedListRepository,
						MangoManager,
						_orderSettings,
						_deliveryRulesSettings,
						_nomenclatureSettings,
						_callTaskWorker,
						_employeeRepository,
						_orderRepository,
						_routeListItemRepository,
						_callTaskRepository);
				
				CounterpartyOrdersViewModels.Add(model);
				currentCounterparty = client;
				MangoManager.AddCounterpartyToCall(client.Id);
				CounterpartyOrdersModelsUpdateEvent();
			}
			(sender as IPage).PageClosed -= OnNewCounterpartyPageClosed;
		}

		void OnExistingCounterpartyPageClosed(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var counterpartyNode = e.SelectedNodes.First() as CounterpartyJournalNode;
			Counterparty client = _uow.GetById<Counterparty>(counterpartyNode.Id);
			if(!CounterpartyOrdersViewModels.Any(c => c.Client.Id == client.Id)) {
				if(_interactiveService.Question($"Добавить телефон к контрагенту {client.Name} ?", "Телефон контрагента")) 
				{
					var phone = ActiveCall.Phone;
					phone.Counterparty = client;
					_uow.Save(phone);
					_uow.Commit();
				}

				var model =
					new CounterpartyOrderViewModel(
						client,
						_unitOfWorkFactory,
						_tdiNavigation,
						_routedListRepository,
						MangoManager,
						_orderSettings,
						_deliveryRulesSettings,
						_nomenclatureSettings,
						_callTaskWorker,
						_employeeRepository,
						_orderRepository,
						_routeListItemRepository,
						_callTaskRepository);
				
				CounterpartyOrdersViewModels.Add(model);
				currentCounterparty = client;
				MangoManager.AddCounterpartyToCall(client.Id);
				CounterpartyOrdersModelsUpdateEvent();
			}
		}

		public void NewOrderCommand()
		{
			if (currentCounterparty.IsForRetail)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заказ поступает от контрагента дистрибуции");
			}

			var model = CounterpartyOrdersViewModels.Find(m => m.Client.Id == currentCounterparty.Id);

			if(model.IsDeliveryPointChoiceRequired && model.DeliveryPoint == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, 
					$"У клиента несколько точек доставки с телефоном {ActiveCall.CallerNumberText}, выберите одну из точек доставки.");

				return;
			}

			var contactPhone = currentCounterparty.Phones?.FirstOrDefault(p => p.DigitsNumber == ActiveCall.Phone.DigitsNumber);

			if(contactPhone == null)
			{
				contactPhone = model.DeliveryPoint?.Phones?.FirstOrDefault(p => p.DigitsNumber == ActiveCall.Phone.DigitsNumber);
			}

			IPage page = _tdiNavigation.OpenTdiTab<OrderDlg, Counterparty, Phone>(null, currentCounterparty, contactPhone);
			page.PageClosed += (s, e) => model.RefreshOrders?.Invoke();
		}

		public void AddComplainCommand()
		{
			var viewModel = _tdiNavigation.OpenViewModel<CreateComplaintViewModel, IEntityUoWBuilder, string>(
				null, EntityUoWBuilder.ForCreate(), "+7" + ActiveCall.Phone.Number).ViewModel;
			viewModel.SetCounterparty(currentCounterparty.Id);
		}

		public void BottleActCommand()
		{
			var parameters = new Vodovoz.Reports.RevisionBottlesAndDeposits(
				_reportInfoFactory,
				_orderRepository,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory);
			parameters.SetCounterparty(currentCounterparty);
			ReportViewDlg dialog = _tdiNavigation.OpenTdiTab<ReportViewDlg, IParametersWidget>(null, parameters) as ReportViewDlg;
			parameters.OnUpdate(true);
			
		}

		public void StockBalanceCommand()
		{
			NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel>(null);
		}

		public void CostAndDeliveryIntervalCommand(DeliveryPoint point)
		{
			_tdiNavigation.OpenTdiTab<DeliveryPriceDlg, DeliveryPoint>(null, point);
		}

		#endregion

		public void Dispose()
		{
			if(_counterpartyJournalPage?.ViewModel != null)
			{
				_counterpartyJournalPage.ViewModel.OnEntitySelectedResult -= OnExistingCounterpartyPageClosed;
			}

			foreach(var model in CounterpartyOrdersViewModels)
			{
				model.Dispose();
			}

			_uow?.Dispose();
		}
	}
}
