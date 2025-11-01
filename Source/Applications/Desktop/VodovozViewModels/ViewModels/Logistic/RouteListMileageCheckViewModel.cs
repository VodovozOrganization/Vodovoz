using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Employee;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListMileageCheckViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IEmployeeSettings _employeeSettings2;
		private readonly IEmployeeSettings _employeeSettings1;
		private readonly ITrackRepository _trackRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IErrorReporter _errorReporter;
		private readonly IWageParameterService _wageParameterService;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;

		private bool _canEdit;

		private CallTaskWorker _callTaskWorker;
		private DelegateCommand _acceptCommand;
		private DelegateCommand _acceptFineCommand;
		private DelegateCommand _openMapCommand;
		private DelegateCommand _fromTrackCommand;
		private DelegateCommand _distributeMileageCommand;
		private DelegateCommand _checkDriversRouteListsDebtCommand;
		private readonly IValidationContextFactory _validationContextFactory;

		private readonly IEmployeeSettings _employeeSettings;
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly IEmployeeService _employeeService;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IRouteListService _routeListService;

		public RouteListMileageCheckViewModel(
			IEntityUoWBuilder uowBuilder,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			IDeliveryShiftRepository deliveryShiftRepository,
			IGtkTabsOpener gtkTabsOpener,
			ITrackRepository trackRepository,
			ICallTaskRepository callTaskRepository,
			IEmployeeRepository employeeRepository,
			IOrderRepository orderRepository,
			IErrorReporter errorReporter,
			IWageParameterService wageParameterService,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IValidationContextFactory validationContextFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IEmployeeSettings employeeSettings,
			IEmployeeService employeeService,
			IRouteListProfitabilityController routeListProfitabilityController,
			ICurrentPermissionService currentPermissionService,
			IRouteListService routeListService)
			:base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			TabName = $"Контроль за километражем маршрутного листа №{Entity.Id}";

			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_callTaskRepository = callTaskRepository ?? throw new ArgumentNullException(nameof(callTaskRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_currentPermissionService = currentPermissionService;
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));

			CarEntryViewModel = BuildCarEntryViewModel(lifetimeScope);
			CanCreateRouteListWithoutOrders = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders);

			LogisticianSelectorFactory = employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory();
			DriverSelectorFactory = employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
			ForwarderSelectorFactory = employeeJournalFactory.CreateWorkingForwarderEmployeeAutocompleteSelectorFactory();
			_employeeSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

			DeliveryShifts =
				(deliveryShiftRepository ?? throw new ArgumentNullException(nameof(deliveryShiftRepository))).ActiveShifts(UoW);

			RouteListItems = GenerateRouteListItems();

			ConfigureValidationContext();

			ConfigureAndCheckPermissions();
		}

		public IEntityAutocompleteSelectorFactory LogisticianSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; }
		public IEntityEntryViewModel CarEntryViewModel { get; }

		public bool CanEdit
		{
			get => _canEdit;
			set
			{
				if(SetField(ref _canEdit, value))
				{
					OnPropertyChanged(nameof(IsAcceptAvailable));
				}
			}
		}

		public bool CanCreateRouteListWithoutOrders { get; }
		public IList<DeliveryShift> DeliveryShifts { get; }
		public IList<RouteListKeepingNode> RouteListItems { get; }

		public bool IsAcceptAvailable => Entity.Status == RouteListStatus.OnClosing || Entity.Status == RouteListStatus.MileageCheck;

		public bool AskSaveOnClose { get; private set; }

		public virtual CallTaskWorker CallTaskWorker =>
			_callTaskWorker ?? (_callTaskWorker = new CallTaskWorker(
				UnitOfWorkFactory,
				CallTaskSingletonFactory.GetInstance(),
				_callTaskRepository,
				_orderRepository,
				_employeeRepository,
				_employeeSettings,
				CommonServices.UserService,
				_errorReporter));

		public DelegateCommand AcceptCommand =>
			_acceptCommand ?? (_acceptCommand = new DelegateCommand(() =>
			{
				if(!CommonServices.ValidationService.Validate(Entity, ValidationContext))
				{
					return;
				}

				if(Entity.Status == RouteListStatus.Delivered)
				{
					ChangeStatusAndCreateTaskFromDelivered();
					OnPropertyChanged(nameof(CanEdit));
				}

				if(!_routeListService.AcceptMileage(UoW, Entity, CommonServices.ValidationService, CallTaskWorker))
				{
					AskSaveOnClose = false;
					return;
				}

				SaveWithClose();
			}
			));

		public DelegateCommand OpenMapCommand =>
			_openMapCommand ?? (_openMapCommand = new DelegateCommand(() =>
			{
				_gtkTabsOpener.OpenTrackOnMapWnd(Entity.Id);
			}
			));

		public DelegateCommand FromTrackCommand =>
			_fromTrackCommand ?? (_fromTrackCommand = new DelegateCommand(() =>
			{
				var track = _trackRepository.GetTrackByRouteListId(UoW, Entity.Id);
				if(track == null)
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно расчитать растояние, так как в маршрутном листе нет трека");
					return;
				}

				Entity.ConfirmedDistance = track.TotalDistance.HasValue ? (decimal)track.TotalDistance.Value : 0;
			}
			));

		public DelegateCommand AcceptFineCommand =>
			_acceptFineCommand ?? (_acceptFineCommand = new DelegateCommand(() =>
			{
				var page = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

				page.ViewModel.SetRouteListById(Entity.Id);
				page.ViewModel.FineReasonString = "Перерасход топлива";
			}
			));

		public DelegateCommand DistributeMileageCommand =>
			_distributeMileageCommand ?? (_distributeMileageCommand = new DelegateCommand(() =>
			{
				if(HasChanges && !SaveBeforeContinue())
				{
					return;
				}

				var page = NavigationManager.OpenViewModel<RouteListMileageDistributionViewModel, ITdiTabParent, ITdiTab>(
					this,
					TabParent,
					this,
					OpenPageOptions.AsSlave,
					conf =>
					{
						conf.Configure(Entity.Driver.Id, Entity.Date, Entity.Car.FullTitle);
					});

				page.ViewModel.Distributed += Close;
			}
			));

		public DelegateCommand CheckDriversRouteListsDebtCommand =>
			_checkDriversRouteListsDebtCommand ?? (_checkDriversRouteListsDebtCommand = new DelegateCommand(() =>
			{
				if(Entity.Driver != null)
				{
					if(!Entity.IsDriversDebtInPermittedRangeVerification())
					{
						Entity.Driver = null;
					}
				}
			}
			));

		private IEntityEntryViewModel BuildCarEntryViewModel(ILifetimeScope lifetimeScope)
		{
			var carViewModelBuilder = new CommonEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, lifetimeScope);

			var viewModel = carViewModelBuilder
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			viewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		private void ConfigureAndCheckPermissions()
		{
			var currentPermissionService = CommonServices.CurrentPermissionService;
			var canUpdate = currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.IsLogistician) && PermissionResult.CanUpdate;
			var canConfirmMileage = currentPermissionService.ValidatePresetPermission("can_confirm_mileage_for_our_GAZelles_Larguses");

			CanEdit = (canUpdate && canConfirmMileage)
					  || !(Entity.GetCarVersion.IsCompanyCar &&
						   new[] { CarTypeOfUse.GAZelle, CarTypeOfUse.Minivan, CarTypeOfUse.Largus }.Contains(Entity.Car.CarModel.CarTypeOfUse));

			AskSaveOnClose = CanEdit;

			if(!CanEdit)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не достаточно прав. Обратитесь к руководителю.");
			}

			if(CanEdit && Entity.Status != RouteListStatus.Closed)
			{
				if(!Entity.RecountMileage())
				{
					FailInitialize = true;
					return;
				}
			}
		}

		private void ConfigureValidationContext()
		{
			ValidationContext = _validationContextFactory.CreateNewValidationContext(Entity,
				new Dictionary<object, object>
					{
						{nameof(IRouteListRepository), _routeListRepository},
						{nameof(IRouteListItemRepository), _routeListItemRepository},
						{Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders, CanCreateRouteListWithoutOrders},
					});
		}

		private IList<RouteListKeepingNode> GenerateRouteListItems()
		{
			var items = new List<RouteListKeepingNode>();

			foreach(var item in Entity.Addresses)
			{
				items.Add(new RouteListKeepingNode { RouteListItem = item });
			}

			items.Sort((x, y) =>
			{
				if(x.RouteListItem.StatusLastUpdate.HasValue && y.RouteListItem.StatusLastUpdate.HasValue)
				{
					if(x.RouteListItem.StatusLastUpdate > y.RouteListItem.StatusLastUpdate)
					{
						return 1;
					}

					if(x.RouteListItem.StatusLastUpdate < y.RouteListItem.StatusLastUpdate)
					{
						return -1;
					}
				}

				return 0;
			});

			return items;
		}

		private void ChangeStatusAndCreateTaskFromDelivered()
		{
			_routeListService.ChangeStatusAndCreateTask(
				UoW,
				Entity,
				Entity.GetCarVersion.IsCompanyCar && Entity.Car.CarModel.CarTypeOfUse != CarTypeOfUse.Truck
					? RouteListStatus.MileageCheck
					: RouteListStatus.OnClosing,
				CallTaskWorker
			);
		}

		protected override bool BeforeSave()
		{
			if(!CommonServices.ValidationService.Validate(Entity, ValidationContext))
			{
				return false;
			}

			if(Entity.Status > RouteListStatus.OnClosing)
			{
				if(Entity.FuelOperationHaveDiscrepancy())
				{
					if(!CommonServices.InteractiveService.Question(
						   "Был изменен водитель или автомобиль, при сохранении МЛ баланс по топливу изменится с учетом этих изменений. Продолжить сохранение?"))
					{
						return false;
					}
				}

				Entity.UpdateFuelOperation();
			}

			if(Entity.Status == RouteListStatus.Delivered)
			{
				ChangeStatusAndCreateTaskFromDelivered();
				OnPropertyChanged(nameof(CanEdit));
			}

			Entity.CalculateWages(_wageParameterService);
			return base.BeforeSave();
		}
		
		public void SaveWithClose()
		{
			Save();
			Close(false, CloseSource.Save);
		}

		protected override void AfterSave()
		{
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			UoW.Save(Entity.RouteListProfitability);
			UoW.Commit();
			base.AfterSave();
		}
		
		private void Close(object o, EventArgs args)
		{
			(o as RouteListMileageDistributionViewModel).Distributed -= Close;
			
			Close(false, CloseSource.Self);
		}
	}
}
