using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using Edo.Transport;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Application.Orders.Services.OrderCancellation;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Edo;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.TrueMark;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using VodovozBusiness.Controllers;
using VodovozBusiness.NotificationSenders;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.TrueMark;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Vodovoz
{
	public partial class RouteListKeepingViewModel : EntityTabViewModelBase<RouteList>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private readonly ILogger<RouteListKeepingViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IDeliveryShiftRepository _deliveryShiftRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly IWageParameterService _wageParameterService;
		private readonly IGeneralSettings _generalSettings;
		private readonly IServiceProvider _serviceProvider;
		private readonly IOrderRepository _orderRepository;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IPermissionResult _permissionResult;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IRouteListService _routeListService;

		private Employee _previousForwarder = null;

		private readonly ViewModelEEVMBuilder<Car> _carViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _driverViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _forwarderViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _logisticianViewModelEEVMBuilder;
		private readonly IDictionary<int, (bool Pushed, PrimaryEdoRequest Request)> _createdOrderEdoRequests =
			new Dictionary<int, (bool Pushed, PrimaryEdoRequest Request)>();
		private readonly List<Action> _cancellationRequestActions = new List<Action>();
		private readonly IEdoSettings _edoSettings;
		private readonly MessageService _edoMessageService;
		private readonly ICounterpartyEdoAccountController _edoAccountController;
		private readonly IRouteListChangesNotificationSender _routeListChangesNotificationSender;
		private readonly OrderCancellationService _orderCancellationService;
		private readonly IRouteListItemTrueMarkProductCodesProcessingService _routeListItemTrueMarkProductCodesProcessingService;
		private bool _canClose = true;
		private IEnumerable<object> _selectedRouteListAddressesObjects = Enumerable.Empty<object>();
		private RouteListItemStatus _routeListItemStatusToChange;
		private UndeliveryViewModel _undeliveryViewModel;

		private HashSet<RouteListItem> _routeListItemsToAddCodesFromStagingCodes = new HashSet<RouteListItem>();

		public RouteListKeepingViewModel(
			ILogger<RouteListKeepingViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			IEmployeeRepository employeeRepository,
			IDeliveryShiftRepository deliveryShiftRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			IWageParameterService wageParameterService,
			IGeneralSettings generalSettings,
			IServiceProvider serviceProvider,
			ICallTaskWorker callTaskWorker,
			IOrderRepository orderRepository,
			ITrueMarkRepository trueMarkRepository,
			DeliveryFreeBalanceViewModel deliveryFreeBalanceViewModel,
			ViewModelEEVMBuilder<Car> carViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> forwarderViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> logisticianViewModelEEVMBuilder,
			IEdoSettings edoSettings,
			MessageService messageService,
			ICounterpartyEdoAccountController edoAccountController,
			IRouteListChangesNotificationSender routeListChangesNotificationSender,
			IOrderContractUpdater orderContractUpdater,
			IRouteListService routeListService,
			IRouteListItemTrueMarkProductCodesProcessingService routeListItemTrueMarkProductCodesProcessingService,
			OrderCancellationService orderCancellationService
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_deliveryShiftRepository = deliveryShiftRepository ?? throw new ArgumentNullException(nameof(deliveryShiftRepository));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			CallTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));

			DeliveryFreeBalanceViewModel = deliveryFreeBalanceViewModel ?? throw new ArgumentNullException(nameof(deliveryFreeBalanceViewModel));
			_carViewModelEEVMBuilder = carViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(carViewModelEEVMBuilder));
			_driverViewModelEEVMBuilder = driverViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(driverViewModelEEVMBuilder));
			_forwarderViewModelEEVMBuilder = forwarderViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(forwarderViewModelEEVMBuilder));
			_logisticianViewModelEEVMBuilder = logisticianViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(logisticianViewModelEEVMBuilder));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_edoMessageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_edoAccountController = edoAccountController ?? throw new ArgumentNullException(nameof(edoAccountController));
			_routeListChangesNotificationSender = routeListChangesNotificationSender ?? throw new ArgumentNullException(nameof(routeListChangesNotificationSender));
			_routeListItemTrueMarkProductCodesProcessingService = routeListItemTrueMarkProductCodesProcessingService ?? throw new ArgumentNullException(nameof(routeListItemTrueMarkProductCodesProcessingService));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_orderCancellationService = orderCancellationService ?? throw new ArgumentNullException(nameof(orderCancellationService));

			TabName = $"Ведение МЛ №{Entity.Id}";

			_permissionResult = _currentPermissionService.ValidateEntityPermission(typeof(RouteList));
			AllEditing = Entity.Status == RouteListStatus.EnRoute && _permissionResult.CanUpdate;
			IsUserLogist = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.IsLogistician);
			LogisticanEditing = IsUserLogist && AllEditing;
			IsOrderWaitUntilActive = _generalSettings.GetIsOrderWaitUntilActive;

			CanCreateRouteListWithoutOrders = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders);
			
			ActiveShifts = _deliveryShiftRepository.ActiveShifts(UoW);

			CarViewModel = BuildCarEntryViewModel();
			DriverViewModel = BuildDriverEntryViewModel();
			ForwarderViewModel = BuildForwarderEntryViewModel();
			LogisticianViewModel = BuildLogisticianEntryViewModel();

			Entity.ObservableAddresses.ElementAdded += OnObservableAddressesElementAdded;
			Entity.ObservableAddresses.ElementRemoved += OnObservableAddressesElementRemoved;
			Entity.ObservableAddresses.ElementChanged += OnObservableAddressesElementChanged;

			//Заполняем информацию о бутылях
			UpdateBottlesSummaryInfo();

			UpdateNodes();

			CreateInitialRouteListItemStatuses();

			SetPropertyChangeRelation(rl => rl.Status,
				() => CanReturnRouteListToEnRouteStatus,
				() => CanSave,
				() => CanComplete);

			SaveCommand = new DelegateCommand(SaveAndClose, () => CanSave);
			CancelCommand = new DelegateCommand(() => Close(true, CloseSource.Cancel));
			RefreshCommand = new DelegateCommand(RefreshCommandHandler, () => AllEditing);
			CreateFineCommand = new DelegateCommand(CreateFineCommandHandler, () => AllEditing);
			ReturnToEnRouteStatus = new DelegateCommand(Entity.RollBackEnRouteStatus, () => CanReturnRouteListToEnRouteStatus);
			CallMadenCommand = new DelegateCommand(CallMadenHandler, () => AllEditing);
			ChangeDeliveryTimeCommand = new DelegateCommand(ChangeDeliveryTimeHandler, () => CanChangeDeliveryTime);
			SetStatusCompleteCommand = new DelegateCommand(SetStatusCompleteHandler, () => CanComplete);
			ReDeliverCommand = new DelegateCommand(ReDeliverHandler, () => Entity.CanChangeStatusToDeliveredWithIgnoringAdditionalLoadingDocument);
			OpenOrderCodesCommand = new DelegateCommand(() => OpenOrderCodesDialog(),() => CanOpenOrderCodes());
			OpenOrderCodesCommand.CanExecuteChangedWith(this, x => x.SelectedRouteListAddressesObjects);
		}

		private void CreateInitialRouteListItemStatuses()
		{
			foreach(var item in Items)
			{
				item.InitialRouteListItemStatusIsInUndeliveryStatuses = RouteListItem.GetUndeliveryStatuses().Contains(item.RouteListItem.Status);
			}
		}

		public Func<Order, IUnitOfWork, RouteListItemStatus, ITdiTab> UndeliveryOpenDlgAction { get; set; }
		
		public virtual ICallTaskWorker CallTaskWorker { get; private set; }

		public IEnumerable<RouteListKeepingItemNode> SelectedRouteListAddresses
		{
			get => SelectedRouteListAddressesObjects.Cast<RouteListKeepingItemNode>();
			set => SelectedRouteListAddressesObjects = value;
		}

		public IEnumerable<object> SelectedRouteListAddressesObjects
		{
			get => _selectedRouteListAddressesObjects;
			set
			{
				if(SetField(ref _selectedRouteListAddressesObjects, value))
				{
					OnPropertyChanged(() => CanComplete);
					OnPropertyChanged(() => CanChangeDeliveryTime);
				}
			}
		}

        public string BottlesInfo { get; private set; }
		public GenericObservableList<RouteListKeepingItemNode> Items { get; private set; } = new GenericObservableList<RouteListKeepingItemNode>();

		#region EEVMs

		public IEntityEntryViewModel CarViewModel { get; }
		public IEntityEntryViewModel DriverViewModel { get; }
		public IEntityEntryViewModel ForwarderViewModel { get; }
		public IEntityEntryViewModel LogisticianViewModel { get; }

		#endregion EEVMs

		public DeliveryFreeBalanceViewModel DeliveryFreeBalanceViewModel { get; }

		//2 уровня доступа к виджетам, для всех и для логистов.
		public bool LogisticanEditing { get; }
		public bool IsUserLogist { get; }

		public bool IsOrderWaitUntilActive { get; }

		public bool CanSave => IsCanClose && AllEditing;
		public bool CanCancel => IsCanClose;
		public bool CanCreateRouteListWithoutOrders { get; }
		public bool CanComplete => AllEditing && SelectedRouteListAddresses.Any();

		[PropertyChangedAlso(nameof(CanSave), nameof(CanCancel))]
		public bool IsCanClose
		{
			get => _canClose;
			set => SetField(ref _canClose, value);
		}

		public bool AllEditing { get; }

		public bool CanChangeForwarder => LogisticanEditing && Entity.CanAddForwarder;

		public bool CanReturnRouteListToEnRouteStatus =>
			Entity.Status == RouteListStatus.OnClosing
			&& IsUserLogist
			&& _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanReturnRouteListToEnRouteStatus);

		public bool CanChangeDeliveryTime => SelectedRouteListAddresses.Count() == 1
			&& _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanChangeDeliveryTime)
			&& AllEditing;

		public IList<DeliveryShift> ActiveShifts { get; }
		public bool AskSaveOnClose => _permissionResult.CanUpdate;

		public override bool HasChanges
		{
			get
			{
				if(Items.All(x => x.Status != RouteListItemStatus.EnRoute))
				{
					return true; //Хак, чтобы вылезало уведомление о закрытии маршрутного листа, даже если ничего не меняли.
				}

				return base.HasChanges;
			}
		}

		#region Commands

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand RefreshCommand { get; }
		public DelegateCommand CreateFineCommand { get; }
		public DelegateCommand ReturnToEnRouteStatus { get; }
		public DelegateCommand CallMadenCommand { get; }
		public DelegateCommand ChangeDeliveryTimeCommand { get; }
		public DelegateCommand SetStatusCompleteCommand { get; }
		public DelegateCommand ReDeliverCommand { get; }
		public DelegateCommand OpenOrderCodesCommand { get; }

		#endregion Commands

		#region EEVMBuilding

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var viewModel = _carViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Car)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.UseViewModelDialog<CarViewModel>()
				.Finish();

			viewModel.CanViewEntity = _currentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		private IEntityEntryViewModel BuildDriverEntryViewModel()
		{
			var viewModel = _driverViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Status = EmployeeStatus.IsWorking;
					filter.RestrictCategory = EmployeeCategory.driver;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			viewModel.Changed += OnDriverChanged;

			return viewModel;
		}

		private IEntityEntryViewModel BuildForwarderEntryViewModel()
		{
			var viewModel = _forwarderViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Forwarder)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Status = EmployeeStatus.IsWorking;
					filter.RestrictCategory = EmployeeCategory.forwarder;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			viewModel.Changed += OnForwarderChanged;

			return viewModel;
		}

		#endregion EEVMBuilding

		public void SelectOrdersById(int[] selectedOrderIds)
		{
			SelectedRouteListAddresses = Items
				.Where(x => selectedOrderIds.Contains(x.RouteListItem.Order.Id))
				.ToArray();
		}

		private void OnDriverChanged(object sender, EventArgs e)
		{
			if(Entity.Driver != null)
			{
				if(!Entity.IsDriversDebtInPermittedRangeVerification())
				{
					Entity.Driver = null;
				}
			}
		}

		private IEntityEntryViewModel BuildLogisticianEntryViewModel()
		{
			var viewModel = _logisticianViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Logistician)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			return viewModel;
		}

		private void UpdateBottlesSummaryInfo()
		{
			string bottles = null;
			int completedBottles = Entity.Addresses
				.Where(x => x != null && x.Status == RouteListItemStatus.Completed)
				.Sum(x => x.Order.Total19LBottlesToDeliver);

			int canceledBottles = Entity.Addresses
				.Where(x => 
					x != null
					&& (x.Status == RouteListItemStatus.Canceled
						|| x.Status == RouteListItemStatus.Overdue
						|| x.Status == RouteListItemStatus.Transfered))
				.Sum(x => x.Order.Total19LBottlesToDeliver);

			int enrouteBottles = Entity.Addresses
				.Where(x => x != null && x.Status == RouteListItemStatus.EnRoute)
				.Sum(x => x.Order.Total19LBottlesToDeliver);

			bottles = "<b>Всего 19л. бутылей в МЛ:</b>\n";
			bottles += $"Выполнено: <b>{completedBottles}</b>\n";
			bottles += $" Отменено: <b>{canceledBottles}</b>\n";
			bottles += $" Осталось: <b>{enrouteBottles}</b>\n";
			BottlesInfo = bottles;
		}

		private void OnObservableAddressesElementAdded(object aList, int[] aIdx)
		{
			UpdateBottlesSummaryInfo();
		}

		private void OnObservableAddressesElementRemoved(object aList, int[] aIdx, object aObject)
		{
			UpdateBottlesSummaryInfo();
		}

		private void OnObservableAddressesElementChanged(object aList, int[] aIdx)
		{
			UpdateBottlesSummaryInfo();
		}

		public string GetLastCallTime(DateTime? lastCall)
		{
			if(lastCall == null)
			{
				return "Водителю еще не звонили.";
			}

			if(lastCall.Value.Date == Entity.Date)
			{
				return $"Последний звонок был в {lastCall:t}";
			}

			return $"Последний звонок был {lastCall:g}";
		}

		public void UpdateNodes()
		{
			var emptyDP = new List<string>();

			Items.ForEach(i => i.StatusChanged -= OnRouteListAddressNodeStatusChanged);
			Items.Clear();

			foreach(var item in Entity.Addresses.Where(x => x != null))
			{
				Items.Add(new RouteListKeepingItemNode { RouteListItem = item });

				if(item.Order.DeliveryPoint == null)
				{
					emptyDP.Add($"Для заказа {item.Order.Id} не определена точка доставки.");
				}
			}

			if(emptyDP.Any())
			{
				var message = string.Join(Environment.NewLine, emptyDP);
				message += Environment.NewLine + "Необходимо добавить точки доставки или сохранить вышеуказанные заказы снова.";
				_interactiveService.ShowMessage(ImportanceLevel.Error, message, "Ошибка");
				FailInitialize = true;
				return;
			}

			Items.ForEach(i => i.StatusChanged += OnRouteListAddressNodeStatusChanged);

			Items = new GenericObservableList<RouteListKeepingItemNode>(Items);
		}

		private void OnRouteListAddressNodeStatusChanged(object sender, StatusChangedEventArgs e)
		{
			_routeListItemStatusToChange = e.NewStatus;

			var rli = sender as RouteListKeepingItemNode;

			if(rli is null)
			{
				return;
			}

			if(!CanCompleteAddressByNewEdoProcess(rli, _routeListItemStatusToChange, out var message))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
				return;
			}
			
			if(RouteListItem.GetUndeliveryStatuses().Contains(_routeListItemStatusToChange))
			{
				if(rli.InitialRouteListItemStatusIsInUndeliveryStatuses
					&& rli.RouteListItemStatusHasChangedToCompeteStatus
					&& RouteListItem.GetUndeliveryStatuses().Contains(_routeListItemStatusToChange))
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning,
						"Вы вернули отменённый заказ  в статус \"Выполнен\" и была создана автоотмена автопереноса.\n" +
						"Если всё же снова хотите отменить данный заказ - переоткройте диалог.");

					return;
				}

				var permit = _orderCancellationService.CanCancelOrder(UoW, rli.RouteListItem.Order);
				switch(permit.Type)
				{
					case OrderCancellationPermitType.AllowCancelDocflow:
						if(permit.EdoTaskToCancellationId == null)
						{
							throw new InvalidOperationException("Для аннулирования документооборота должен быть указан идентификатор ЭДО задачи.");
						}
						_orderCancellationService.CancelDocflowByUser(
							$"Отмена заказа №{rli.RouteListItem.Order.Id}",
							permit.EdoTaskToCancellationId.Value
						);
						return;
					case OrderCancellationPermitType.AllowCancelOrder:
						break;
					case OrderCancellationPermitType.Deny:
					default:
						return;
				}

				_undeliveryViewModel = NavigationManager.OpenViewModel<UndeliveryViewModel>(
					this,
					OpenPageOptions.AsSlave,
					vm =>
					{
						vm.Saved += OnUndeliveryViewModelSaved;
						vm.Initialize(rli.RouteListItem.RouteList.UoW, rli.RouteListItem.Order.Id, cancellationPermit: permit);
					}
					).ViewModel;

				return;
			}

			var validationContext = new ValidationContext(Entity, _serviceProvider, new Dictionary<object, object>
			{
				{ "uowFactory", UnitOfWorkFactory },
			});

			var canCreateSeveralOrdersValidationResult =
				rli.RouteListItem.Order.ValidateCanCreateSeveralOrderForDateAndDeliveryPoint(validationContext);

			if(canCreateSeveralOrdersValidationResult != ValidationResult.Success)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Нельзя перевести адрес в статус \"{_routeListItemStatusToChange.GetEnumTitle()}\": {canCreateSeveralOrdersValidationResult.ErrorMessage} ");

				return;
			}

			if(_routeListItemStatusToChange == RouteListItemStatus.Completed)
			{
				rli.RouteListItemStatusHasChangedToCompeteStatus = true;
			}

			rli.UpdateStatus(_routeListService, _routeListItemStatusToChange, CallTaskWorker);
			TryUpdateCreatedEdoRequests(rli, _routeListItemStatusToChange);
		}

		private void TryUpdateCreatedEdoRequests(RouteListKeepingItemNode rli, RouteListItemStatus addressStatus)
		{
			if(!_edoSettings.NewEdoProcessing)
			{
				return;
			}

			if(HasEdoRequest(rli.RouteListItem.Order.Id))
			{
				return;
			}

			if(!_orderRepository.IsAllDriversScannedCodesInOrderProcessed(UoW, rli.RouteListItem.Order.Id).GetAwaiter().GetResult())
			{
				return;
			}

			var request = CreateOrderRequest(rli, rli.RouteListItem.TrueMarkCodes);
			UpdateCreatedEdoRequests(request, addressStatus);
		}

		private bool CanCompleteAddressByNewEdoProcess(
			RouteListKeepingItemNode rli,
			RouteListItemStatus newStatus,
			out string message)
		{
			message = null;
			var order = rli.RouteListItem.Order;

			if(newStatus != RouteListItemStatus.Completed
				&& _routeListItemsToAddCodesFromStagingCodes.Contains(rli.RouteListItem))
			{
				_routeListItemsToAddCodesFromStagingCodes.Remove(rli.RouteListItem);
			}

			if(newStatus == RouteListItemStatus.Completed
				&& order.IsOrderContainsIsAccountableInTrueMarkItems
				&& !_currentPermissionService.ValidatePresetPermission(
				   Core.Domain.Permissions.LogisticPermissions.RouteListItem.CanSetCompletedStatusWhenNotAllTrueMarkCodesAdded))
			{
				int requiredCodesCount = _trueMarkRepository.GetCodesRequiredByOrder(UoW, order.Id);

				var driverCodes = _trueMarkRepository.GetCodesFromDriverByOrder(UoW, order.Id);

				bool isAllDriverTrueMarkCodesAdded = driverCodes.Count() == requiredCodesCount;

				if(isAllDriverTrueMarkCodesAdded)
				{
					return true;
				}

				if((order.IsNeedIndividualSetOnLoad(_edoAccountController) || order.IsNeedIndividualSetOnLoadForTender)
				   && !_orderRepository.IsOrderCarLoadDocumentLoadOperationStateDone(UoW, order.Id))
				{
					message = $"Заказ {order.Id} не может быть переведен в статус \"Доставлен\", " +
						"т.к. данный заказ является сетевым, либо госзаказом, но документ погрузки не находится в статусе \"Погрузка завершена\"";

					return false;
				}

				var isOrderForResaleAndMustBeScannedByDriver =
					order.IsOrderForResale
					&& !order.IsNeedIndividualSetOnLoad(_edoAccountController);

				var isOrderForTenderAndMustBeScannedByDriver =
					order.IsOrderForTender
					&& order.Client.OrderStatusForSendingUpd == OrderStatusForSendingUpd.Delivered
					&& !order.IsNeedIndividualSetOnLoad(_edoAccountController);

				if(isOrderForResaleAndMustBeScannedByDriver || isOrderForTenderAndMustBeScannedByDriver)
				{
					var isAllStagingCodesAddedResult =
						_routeListItemTrueMarkProductCodesProcessingService.IsAllStagingTrueMarkCodesAddedToRouteListItem(UoW, rli.RouteListItem).GetAwaiter().GetResult();

					if(isAllStagingCodesAddedResult.IsFailure)
					{
						var orderPurpose = order.IsOrderForResale ? "на перепродажу" : "на тендер";
						message =
							$"Заказ {order.Id} не может быть переведен в статус \"Доставлен\", "
							+ $"т.к. данный заказ на {orderPurpose}, но количество добавленных кодов не соответствует заказу";
						return false;
					}

					_routeListItemsToAddCodesFromStagingCodes.Add(rli.RouteListItem);
				}
			}

			return true;
		}

		private void OnUndeliveryViewModelSaved(object sender, Application.Orders.UndeliveryOnOrderCloseEventArgs e)
		{
			var address = Items
				.Where(x => x.RouteListItem.Order.Id == e.UndeliveredOrder.OldOrder.Id)
				.FirstOrDefault();

			address.UpdateStatus(_routeListService,  _routeListItemStatusToChange, CallTaskWorker);
			TryUpdateCreatedEdoRequests(address, _routeListItemStatusToChange);
			UoW.Save(address.RouteListItem);

			var notificationRequest = new NotificationRouteListChangesRequest
			{
				OrderId = e.UndeliveredOrder.OldOrder.Id ,
				PushNotificationDataEventType = PushNotificationDataEventType.RouteListContentChanged
			};

			var result = _routeListChangesNotificationSender.NotifyOfRouteListChanged(notificationRequest).GetAwaiter().GetResult();

			if(!result.IsSuccess)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					string.Join(", ",
					result.Errors
					.Where(x => x.Code == Errors.Logistics.RouteListErrors.RouteListItem.TransferTypeNotSet)
					.Select(x => x.Message))
					);
			}

			var allowCancellation = e.CancellationPermit.Type == OrderCancellationPermitType.AllowCancelOrder;
			var hasEdoTaskToCancellationId = e.CancellationPermit.EdoTaskToCancellationId != null;
			if(allowCancellation && hasEdoTaskToCancellationId)
			{
				_cancellationRequestActions.Add(() => _orderCancellationService.AutomaticCancelDocflow(
					UoW,
					$"Отмена заказа №{e.UndeliveredOrder.OldOrder.Id}",
					e.CancellationPermit.EdoTaskToCancellationId.Value
				));
			}
		}

		private bool AddProductCodesToAllCompletedRouteListItemsFromStagingCodes(out string message)
		{
			message = string.Empty;
			var routeListItemsWithAddedCodes = new List<RouteListItem>();

			foreach(var routeListItem in _routeListItemsToAddCodesFromStagingCodes)
			{
				if(!AddProductCodesToRouteListItemFromStagingCodes(routeListItem, out var addCodesMessage))
				{
					message = addCodesMessage;
					return false;
				}
				routeListItemsWithAddedCodes.Add(routeListItem);

				if(_createdOrderEdoRequests.TryGetValue(routeListItem.Order.Id, out var requestData))
				{
					var request = requestData.Request;
					request.ProductCodes.Clear();

					foreach(var code in routeListItem.TrueMarkCodes)
					{
						request.ProductCodes.Add(code);
					}
				}
			}

			foreach(var routeListItem in routeListItemsWithAddedCodes)
			{
				_routeListItemsToAddCodesFromStagingCodes.Remove(routeListItem);
			}

			return true;
		}

		private bool AddProductCodesToRouteListItemFromStagingCodes(RouteListItem routeListItem, out string message)
		{
			message = string.Empty;
			var order = routeListItem.Order;

			var addCodesResult = _routeListItemTrueMarkProductCodesProcessingService
				.AddProductCodesToRouteListItemAndDeleteStagingCodes(UoW, routeListItem)
				.GetAwaiter().GetResult();

			if(addCodesResult.IsFailure)
			{
				var orderPurpose = order.IsOrderForResale ? "на перепродажу" : "на тендер";
				message = $"Заказ {order.Id} не может быть переведен в статус \"Доставлен\", "
					+ $"т.к. данный заказ на {orderPurpose}, но количество добавленных кодов не соответствует заказу";

				return false;
			}

			return true;
		}

		private void OnForwarderChanged(object sender, EventArgs e)
		{
			var newForwarder = Entity.Forwarder;

			if(Entity.Status == RouteListStatus.OnClosing
				&& ((_previousForwarder == null && newForwarder != null)
					|| (_previousForwarder != null && newForwarder == null)))
			{
				Entity.RecalculateAllWages(_wageParameterService);
			}

			_previousForwarder = Entity.Forwarder;
		}

		#region implemented abstract members of OrmGtkDialogBase

		public bool CanClose()
		{
			if(!IsCanClose)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Дождитесь завершения работы задачи и повторите");
			}

			return IsCanClose;
		}

		protected override bool BeforeValidation()
		{
			
			ValidationContext = new ValidationContext(Entity, _serviceProvider, new Dictionary<object, object>
			{
				{ "uowFactory", UnitOfWorkFactory },
				{Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders, CanCreateRouteListWithoutOrders},
			});

			return base.BeforeValidation();
		}

		protected override bool BeforeSave()
		{
			try
			{
				IsCanClose = false;

				var isCodesAdded = AddProductCodesToAllCompletedRouteListItemsFromStagingCodes(out var message);

				if(!isCodesAdded)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
					return false;
				}

				foreach(var node in Items.Where(x => x.PaymentTypeHasChanged))
				{
					var order = node.RouteListItem.Order;
					var newPaymentType = node.PaymentType;
					order.Contract = null;
					order.UpdatePaymentType(node.PaymentType, _orderContractUpdater);
				}

				UoWGeneric.Save();

				_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);

				UoW.Save(Entity.RouteListProfitability);
				
				foreach(var keyPairValue in _createdOrderEdoRequests)
				{
					UoW.Save(keyPairValue.Value.Request);
				}

				UoW.Commit();

				var changedItems = Items
					.Where(item => item.ChangedDeliverySchedule || item.HasChanged)
					.ToList();

				if(changedItems.Count == 0)
				{
					return true;
				}

				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoWGeneric);

				if(currentEmployee == null)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Info,
						"Ваш пользователь не привязан к сотруднику, уведомления об изменениях в маршрутном листе не будут отправлены водителю.");
				}

				Entity.CalculateWages(_wageParameterService);
			}
			catch(NHibernate.Exceptions.GenericADOException ex) when(
				ex.InnerException?.Message.Contains("Lock wait timeout exceeded") == true ||
				ex.InnerException?.Message.Contains("Deadlock found when trying to get lock") == true)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Не удалось сохранить изменения, так как маршрутный лист редактируется другим пользователем. " +
					"Пожалуйста, повторите попытку позже.");
				return false;
			}
			finally
			{
				IsCanClose = true;
			}
			
			return base.BeforeSave();
		}

		public override bool Save(bool close)
		{
			if(!base.Save(false))
			{
				return false;
			}

			if(close)
			{
				Close(false, CloseSource.Save);
			}

			return true;
		}

		protected override void AfterSave()
		{
			if(_createdOrderEdoRequests.Any())
			{
				Task.Run(async() =>
				{
					foreach(var keyPairValue in _createdOrderEdoRequests)
					{
						var value = keyPairValue.Value;

						if(value.Pushed)
						{
							continue;
						}
						
						await _edoMessageService.PublishEdoRequestCreatedEvent(value.Request.Id);
						value.Pushed = true;
					}
				});
			}

			if(_cancellationRequestActions.Any())
			{
				foreach(var cancellationAction in _cancellationRequestActions)
				{
					cancellationAction.Invoke();
				}

				_cancellationRequestActions.Clear();
			}
		}

		#endregion

		protected void RefreshCommandHandler()
		{
			bool hasChanges = Items.Any(item => item.HasChanged);

			if(!hasChanges || _interactiveService.Question("Вы действительно хотите обновить список заказов? Внесенные изменения будут утрачены."))
			{
				UoW.Session.Refresh(Entity);
				UpdateNodes();
			}
		}

		protected void ChangeDeliveryTimeHandler()
		{
			if(_currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanChangeDeliveryTime))
			{
				if(SelectedRouteListAddresses.Count() != 1)
				{
					return;
				}

				var selectedAddress = SelectedRouteListAddresses
					.FirstOrDefault();

				NavigationManager.OpenViewModel<DeliveryScheduleJournalViewModel>(this, OpenPageOptions.AsSlave, viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnEntitySelectedResult += (s, args) =>
					{
						if(!(args.SelectedNodes.FirstOrDefault() is DeliveryScheduleJournalNode selectedResult))
						{
							return;
						}

						var selectedEntity = UoW.GetById<DeliverySchedule>(selectedResult.Id);

						if(selectedAddress.RouteListItem.Order.DeliverySchedule.Id != selectedEntity.Id)
						{
							selectedAddress.RouteListItem.Order.DeliverySchedule = selectedEntity;
							selectedAddress.ChangedDeliverySchedule = true;
						}
					};
				});
			}
		}

		protected void SetStatusCompleteHandler()
		{
			var cantSetCompleteMessages = new StringBuilder();
			
			foreach(var item in SelectedRouteListAddresses)
			{
				if(item.Status == RouteListItemStatus.Transfered)
				{
					continue;
				}

				const RouteListItemStatus newStatus = RouteListItemStatus.Completed;

				if(!CanCompleteAddressByNewEdoProcess(item, newStatus, out var message))
				{
					cantSetCompleteMessages.AppendLine(message);
					continue;
				}
				
				_routeListService.ChangeAddressStatusAndCreateTask(UoW, Entity, item.RouteListItem.Id, newStatus, CallTaskWorker);
				TryUpdateCreatedEdoRequests(item, newStatus);
			}

			if(cantSetCompleteMessages.Length > 0)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, cantSetCompleteMessages.ToString());
			}
		}
		
		private void UpdateCreatedEdoRequests(
			PrimaryEdoRequest request,
			RouteListItemStatus addressStatus = RouteListItemStatus.Completed)
		{
			var hasRequest = _createdOrderEdoRequests.ContainsKey(request.Order.Id);
			
			switch (hasRequest)
			{
				case true when addressStatus != RouteListItemStatus.Completed:
					_createdOrderEdoRequests.Remove(request.Order.Id);
					return;
				case true:
				case false when addressStatus != RouteListItemStatus.Completed:
					return;
				default:
					_createdOrderEdoRequests.Add(request.Order.Id, (false, request));
					break;
			}
		}

		private static PrimaryEdoRequest CreateOrderRequest(
			RouteListKeepingItemNode item,
			IObservableList<RouteListItemTrueMarkProductCode> codes)
		{
			return new PrimaryEdoRequest
			{
				Order = item.RouteListItem.Order,
				Source = CustomerEdoRequestSource.Manual,
				Time = DateTime.Now,
				DocumentType = EdoDocumentType.UPD,
				Type = CustomerEdoRequestType.Order,
				ProductCodes = new ObservableList<TrueMarkProductCode>(codes)
			};
		}

		private bool HasEdoRequest(int orderId)
		{
			return UoW.GetAll<FormalEdoRequest>()
				.FirstOrDefault(x => x.Order.Id == orderId) != null;
		}

		protected void CreateFineCommandHandler()
		{
			var page = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

			page.ViewModel.SetRouteListById(Entity.Id);
		}

		protected void CallMadenHandler()
		{
			Entity.LastCallTime = DateTime.Now;
		}

		protected void ReDeliverHandler()
		{
			_routeListService.UpdateStatus(UoW, Entity, isIgnoreAdditionalLoadingDocument: true);
		}

		protected void OpenOrderCodesDialog()
		{
			if(!CanOpenOrderCodes())
			{
				return;
			}
			var selectedAddress = SelectedRouteListAddressesObjects.FirstOrDefault() as RouteListKeepingItemNode;
			NavigationManager.OpenViewModel<OrderCodesViewModel, int>(null, selectedAddress.RouteListItem.Order.Id, OpenPageOptions.IgnoreHash);
		}

		protected bool CanOpenOrderCodes()
		{
			if(SelectedRouteListAddressesObjects.Count() > 1)
			{
				return false;
			}

			var selectedAddress = SelectedRouteListAddressesObjects.FirstOrDefault() as RouteListKeepingItemNode;
			if(selectedAddress == null || selectedAddress.RouteListItem == null)
			{
				return false;
			}
			return true;
		}

		public override void Dispose()
		{
			if(_undeliveryViewModel != null)
			{
				_undeliveryViewModel.Saved -= OnUndeliveryViewModelSaved;
			}
			Entity.ObservableAddresses.ElementAdded -= OnObservableAddressesElementAdded;
			Entity.ObservableAddresses.ElementRemoved -= OnObservableAddressesElementRemoved;
			Entity.ObservableAddresses.ElementChanged -= OnObservableAddressesElementChanged;

			base.Dispose();
		}
	}
}
