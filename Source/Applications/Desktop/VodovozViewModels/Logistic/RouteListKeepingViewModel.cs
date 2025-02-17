using Autofac;
using Gamma.Utilities;
using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
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
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Common;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz
{
	public partial class RouteListKeepingViewModel : EntityTabViewModelBase<RouteList>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IDeliveryShiftRepository _deliveryShiftRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly IWageParameterService _wageParameterService;
		private readonly IGeneralSettings _generalSettings;
		private readonly IServiceProvider _serviceProvider;
		private readonly IOrderRepository _orderRepository;
		private readonly IPermissionResult _permissionResult;

		private Employee _previousForwarder = null;

		private readonly ViewModelEEVMBuilder<Car> _carViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _driverViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _forwarderViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _logisticianViewModelEEVMBuilder;
		private bool _canClose = true;
		private IEnumerable<object> _selectedRouteListAddressesObjects = Enumerable.Empty<object>();
		private RouteListItemStatus _routeListItemStatusToChange;
		private UndeliveryViewModel _undeliveryViewModel;

		public RouteListKeepingViewModel(
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
			DeliveryFreeBalanceViewModel deliveryFreeBalanceViewModel,
			ViewModelEEVMBuilder<Car> carViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> forwarderViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> logisticianViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
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

			DeliveryFreeBalanceViewModel = deliveryFreeBalanceViewModel ?? throw new ArgumentNullException(nameof(deliveryFreeBalanceViewModel));
			_carViewModelEEVMBuilder = carViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(carViewModelEEVMBuilder));
			_driverViewModelEEVMBuilder = driverViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(driverViewModelEEVMBuilder));
			_forwarderViewModelEEVMBuilder = forwarderViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(forwarderViewModelEEVMBuilder));
			_logisticianViewModelEEVMBuilder = logisticianViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(logisticianViewModelEEVMBuilder));
			TabName = $"Ведение МЛ №{Entity.Id}";

			_permissionResult = _currentPermissionService.ValidateEntityPermission(typeof(RouteList));
			AllEditing = Entity.Status == RouteListStatus.EnRoute && _permissionResult.CanUpdate;
			IsUserLogist = _currentPermissionService.ValidatePresetPermission(Permissions.Logistic.IsLogistician);
			LogisticanEditing = IsUserLogist && AllEditing;
			IsOrderWaitUntilActive = _generalSettings.GetIsOrderWaitUntilActive;

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
			&& _currentPermissionService.ValidatePresetPermission(Permissions.Logistic.RouteList.CanReturnRouteListToEnRouteStatus);

		public bool CanChangeDeliveryTime => SelectedRouteListAddresses.Count() == 1
			&& _currentPermissionService.ValidatePresetPermission(Permissions.Logistic.RouteList.CanChangeDeliveryTime)
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

			if(_routeListItemStatusToChange == RouteListItemStatus.Completed
				&& rli.RouteListItem.Order.IsNeedIndividualSetOnLoad && rli.RouteListItem.Order.IsOrderContainsIsAccountableInTrueMarkItems)
			{
				if(_orderRepository.IsOrderCarLoadDocumentLoadOperationStateDone(UoW, rli.RouteListItem.Order.Id))
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning,
						"Заказ не может быть переведен в статус \"Доставлен\", " +
						"т.к. данный заказ является сетевым, но документ погрузки не находится в статусе \"Погрузка завершена\"");

					return;
				}
			}

			if(_routeListItemStatusToChange == RouteListItemStatus.Completed
				&& rli.RouteListItem.Order.IsOrderForResale && rli.RouteListItem.Order.IsOrderContainsIsAccountableInTrueMarkItems)
			{
				if(!_orderRepository.IsAllRouteListItemTrueMarkProductCodesAddedToOrder(UoW, rli.RouteListItem.Order.Id))
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning,
						"Заказ не может быть переведен в статус \"Доставлен\", " +
						"т.к. данный заказ на перепродажу, но не все коды ЧЗ были добавлены");

					return;
				}
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

				_undeliveryViewModel = NavigationManager.OpenViewModel<UndeliveryViewModel>(
					this,
					OpenPageOptions.AsSlave,
					vm =>
					{
						vm.Saved += OnUndeliveryViewModelSaved;
						vm.Initialize(rli.RouteListItem.RouteList.UoW, rli.RouteListItem.Order.Id);
					}
					).ViewModel;

				return;
			}

			var validationContext = new ValidationContext(Entity, _serviceProvider, new Dictionary<object, object>
			{
				{ "uowFactory", UnitOfWorkFactory }
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

			rli.UpdateStatus(_routeListItemStatusToChange, CallTaskWorker);
		}

		private void OnUndeliveryViewModelSaved(object sender, Application.Orders.UndeliveryOnOrderCloseEventArgs e)
		{
			var address = Items
				.Where(x => x.RouteListItem.Order.Id == e.UndeliveredOrder.OldOrder.Id)
				.FirstOrDefault();

			address.UpdateStatus(_routeListItemStatusToChange, CallTaskWorker);
			UoW.Save(address.RouteListItem);
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
			ValidationContext = new ValidationContext(Entity);

			return base.BeforeValidation();
		}

		protected override bool BeforeSave()
		{
			try
			{
				IsCanClose = false;
				
				UoWGeneric.Save();

				_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);

				UoW.Save(Entity.RouteListProfitability);
				UoW.Commit();

				var changedList = Items
					.Where(item => item.ChangedDeliverySchedule || item.HasChanged)
					.ToList();

				if(changedList.Count == 0)
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
			finally
			{
				IsCanClose = true;
			}
			
			return base.BeforeSave();
		}

		#endregion

		protected void RefreshCommandHandler()
		{
			bool hasChanges = Items.Any(item => item.HasChanged);

			if(!hasChanges || _interactiveService.Question("Вы действительно хотите обновить список заказов? Внесенные изменения будут утрачены."))
			{
				UoWGeneric.Session.Refresh(Entity);
				UpdateNodes();
			}
		}

		protected void ChangeDeliveryTimeHandler()
		{
			if(_currentPermissionService.ValidatePresetPermission(Permissions.Logistic.RouteList.CanChangeDeliveryTime))
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
			foreach(RouteListKeepingItemNode item in SelectedRouteListAddresses)
			{
				if(item.Status == RouteListItemStatus.Transfered)
				{
					continue;
				}

				Entity.ChangeAddressStatusAndCreateTask(UoW, item.RouteListItem.Id, RouteListItemStatus.Completed, CallTaskWorker);
			}
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
			Entity.UpdateStatus(isIgnoreAdditionalLoadingDocument: true);
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
