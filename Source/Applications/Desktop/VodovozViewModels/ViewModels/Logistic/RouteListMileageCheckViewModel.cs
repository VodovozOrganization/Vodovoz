using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.Tdi;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListMileageCheckViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly BaseParametersProvider _baseParametersProvider;
		private readonly ITrackRepository _trackRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IErrorReporter _errorReporter;
		private readonly WageParameterService _wageParameterService;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;

		private bool _canEdit;

		private CallTaskWorker _callTaskWorker;
		private DelegateCommand _acceptCommand;
		private DelegateCommand _acceptFineCommand;
		private DelegateCommand _openMapCommand;
		private DelegateCommand _fromTrackCommand;
		private DelegateCommand _distributeMileageCommand;
		private readonly IValidationContextFactory _validationContextFactory;

		private readonly IUndeliveredOrdersJournalOpener _undeliveryViewOpener;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly IEmployeeService _employeeService;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;

		public RouteListMileageCheckViewModel(
			IEntityUoWBuilder uowBuilder,
			ICommonServices commonServices,
			ICarJournalFactory carJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IDeliveryShiftRepository deliveryShiftRepository,
			IGtkTabsOpener gtkTabsOpener,
			BaseParametersProvider baseParametersProvider,
			ITrackRepository trackRepository,
			ICallTaskRepository callTaskRepository,
			IEmployeeRepository employeeRepository,
			IOrderRepository orderRepository,
			IErrorReporter errorReporter,
			WageParameterService wageParameterService,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IValidationContextFactory validationContextFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			IUndeliveredOrdersJournalOpener undeliveryViewOpener,
			IEmployeeSettings employeeSettings,
			IEmployeeService employeeService,
			IRouteListProfitabilityController routeListProfitabilityController)
			:base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			TabName = $"Контроль за километражем маршрутного листа №{Entity.Id}";

			_baseParametersProvider = baseParametersProvider ?? throw new ArgumentNullException(nameof(baseParametersProvider));
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
			_undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));

			CarSelectorFactory = (carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory)))
				.CreateCarAutocompleteSelectorFactory();

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


		private void ConfigureAndCheckPermissions()
		{
			var currentPermissionService = CommonServices.CurrentPermissionService;
			var canUpdate = currentPermissionService.ValidatePresetPermission("logistican") && PermissionResult.CanUpdate;
			var canConfirmMileage = currentPermissionService.ValidatePresetPermission("can_confirm_mileage_for_our_GAZelles_Larguses");

			CanEdit = (canUpdate && canConfirmMileage)
					  || !(Entity.GetCarVersion.IsCompanyCar &&
						   new[] { CarTypeOfUse.GAZelle, CarTypeOfUse.Largus }.Contains(Entity.Car.CarModel.CarTypeOfUse));

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
						{nameof(IRouteListItemRepository), _routeListItemRepository}
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
			Entity.ChangeStatusAndCreateTask(
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

		public IEntityAutocompleteSelectorFactory CarSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory LogisticianSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; }

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

		public bool IsAcceptAvailable => Entity.Status == RouteListStatus.OnClosing || Entity.Status == RouteListStatus.MileageCheck;
		public bool AskSaveOnClose => CanEdit;
		public IList<DeliveryShift> DeliveryShifts { get; }
		public IList<RouteListKeepingNode> RouteListItems { get; }

		public virtual CallTaskWorker CallTaskWorker =>
			_callTaskWorker ?? (_callTaskWorker = new CallTaskWorker(
				CallTaskSingletonFactory.GetInstance(),
				_callTaskRepository,
				_orderRepository,
				_employeeRepository,
				_baseParametersProvider,
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

					Entity.AcceptMileage(CallTaskWorker);

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
					var fineViewModel = new FineViewModel(
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						_undeliveryViewOpener,
						_employeeService,
						_employeeJournalFactory,
						_employeeSettings,
						CommonServices
					)
					{
						RouteList = Entity,
						FineReasonString = "Перерасход топлива"
					};

					TabParent.AddSlaveTab(this, fineViewModel);
				}
			));

		public DelegateCommand DistributeMileageCommand =>
			_distributeMileageCommand ?? (_distributeMileageCommand = new DelegateCommand(() =>
				{
					if(HasChanges && !SaveBeforeContinue())
					{
						return;
					}

					NavigationManager.OpenViewModel<RouteListMileageDistributionViewModel, IEntityUoWBuilder, ITdiTabParent, ITdiTab>(
						this, EntityUoWBuilder.ForOpen(Entity.Id), TabParent, this, OpenPageOptions.AsSlave);
				}
			));
	}
}
