﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QSReport;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.Views.Mango;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public partial class CounterpartyTalkViewModel : TalkViewModelBase, IDisposable
	{
		private readonly ITdiCompatibilityNavigation _tdiNavigation;
		private readonly IRouteListRepository _routedListRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IParametersProvider _parametersProvider;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly IUnitOfWork _uow;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;

		public List<CounterpartyOrderViewModel> CounterpartyOrdersViewModels { get; private set; } = new List<CounterpartyOrderViewModel>();

		public Counterparty currentCounterparty { get;private set; }
		public event Action CounterpartyOrdersModelsUpdateEvent = () => { };

		public CounterpartyTalkViewModel(
			INavigationManager navigation,
			ITdiCompatibilityNavigation tdinavigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListRepository routedListRepository,
			IInteractiveService interactiveService,
			IOrderParametersProvider orderParametersProvider, 
			MangoManager manager,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			INomenclatureRepository nomenclatureRepository,
			IOrderRepository orderRepository,
			IParametersProvider parametersProvider,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IDeliveryPointJournalFactory deliveryPointJournalFactory) : base(navigation, manager)
		{
			NavigationManager = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_tdiNavigation = tdinavigation ?? throw new ArgumentNullException(nameof(navigation));

			_routedListRepository = routedListRepository;
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_uow = unitOfWorkFactory.CreateWithoutRoot();
			_deliveryPointJournalFactory =
				deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));

			if(ActiveCall.CounterpartyIds.Any())
			{
				var clients = _uow.GetById<Counterparty>(ActiveCall.CounterpartyIds);

				foreach(Counterparty client in clients)
				{
					CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(
						client, unitOfWorkFactory, tdinavigation, routedListRepository, MangoManager, _orderParametersProvider,
						_employeeJournalFactory, _counterpartyJournalFactory, _nomenclatureRepository, _parametersProvider,
						_deliveryRulesParametersProvider);
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
			var page = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			page.ViewModel.OnEntitySelectedResult += ExistingCounterparty_PageClosed;
		}

		private void OnNewCounterpartyPageClosed(object sender, PageClosedEventArgs e)
		{
			if(e.CloseSource == CloseSource.Save)
			{
				Counterparty client = ((sender as TdiTabPage).TdiTab as CounterpartyDlg).Counterparty;
				
				CounterpartyOrderViewModel model = 
					new CounterpartyOrderViewModel(
						client,
						UnitOfWorkFactory.GetDefaultFactory,
						_tdiNavigation,
						_routedListRepository,
						MangoManager,
						_orderParametersProvider,
						_employeeJournalFactory,
						_counterpartyJournalFactory,
						_nomenclatureRepository,
						_parametersProvider,
						_deliveryRulesParametersProvider);
				
				CounterpartyOrdersViewModels.Add(model);
				currentCounterparty = client;
				MangoManager.AddCounterpartyToCall(client.Id);
				CounterpartyOrdersModelsUpdateEvent();
			}
			(sender as IPage).PageClosed -= OnNewCounterpartyPageClosed;
		}

		void ExistingCounterparty_PageClosed(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
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

				CounterpartyOrderViewModel model =
					new CounterpartyOrderViewModel(
						client, UnitOfWorkFactory.GetDefaultFactory, _tdiNavigation, _routedListRepository, MangoManager,
						_orderParametersProvider, _employeeJournalFactory, _counterpartyJournalFactory, _nomenclatureRepository,
						_parametersProvider, _deliveryRulesParametersProvider);
				
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
			page.PageClosed += (sender, e) => { model.RefreshOrders(); };
		}


		public void AddComplainCommand()
		{
			var employeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

			var counterpartySelectorFactory = _counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory();

			var parameters = new Dictionary<string, object> {
				{"client", currentCounterparty},
				{"uowBuilder", EntityUoWBuilder.ForCreate()},
				{ "unitOfWorkFactory", UnitOfWorkFactory.GetDefaultFactory },
				//Autofac: IEmployeeService 
				{"employeeSelectorFactory", employeeSelectorFactory},
				{"counterpartySelectorFactory", counterpartySelectorFactory},
				//Autofac: ICommonServices
				//Autofac: IUserRepository
				{"phone", "+7" + ActiveCall.Phone.Number }
			};
			
			_tdiNavigation.OpenTdiTabOnTdiNamedArgs<CreateComplaintViewModel>(null,parameters);
		}

		public void BottleActCommand()
		{
			var parameters = new Vodovoz.Reports.RevisionBottlesAndDeposits(
				_orderRepository,
				_counterpartyJournalFactory,
				_deliveryPointJournalFactory);
			parameters.SetCounterparty(currentCounterparty);
			ReportViewDlg dialog = _tdiNavigation.OpenTdiTab<ReportViewDlg, IParametersWidget>(null, parameters) as ReportViewDlg;
			parameters.OnUpdate(true);
			
		}

		public void StockBalanceCommand()
		{
			NomenclatureStockFilterViewModel filter = new NomenclatureStockFilterViewModel(new WarehouseSelectorFactory());
			NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel, NomenclatureStockFilterViewModel>(null, filter);
		}

		public void CostAndDeliveryIntervalCommand(DeliveryPoint point)
		{
			_tdiNavigation.OpenTdiTab<DeliveryPriceDlg, DeliveryPoint>(null, point);
		}

		#endregion

		public void Dispose()
		{
			foreach(var model in CounterpartyOrdersViewModels)
			{
				model.Dispose();
			}

			_uow?.Dispose();
		}
	}
}
