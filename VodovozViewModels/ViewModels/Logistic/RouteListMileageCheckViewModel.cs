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
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListMileageCheckViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		#region Поля

		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
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
		private DelegateCommand _distributeMiliageCommand;
		private readonly IValidationContextFactory _validationContextFactory;

		#endregion

		public RouteListMileageCheckViewModel(IEntityUoWBuilder uowBuilder,
			ICommonServices commonServices,
			ICarJournalFactory carJournalFactory,
			IEmployeeJournalFactory employeeFactory,
			IDeliveryShiftRepository deliveryShiftRepository,
			IOrderParametersProvider orderParametersProvider,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
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
			INavigationManager navigationManager)
			:base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			TabName = $"Контроль за километражем маршрутного листа №{Entity.Id}";

			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ??
											   throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_baseParametersProvider = baseParametersProvider ?? throw new ArgumentNullException(nameof(baseParametersProvider));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_callTaskRepository = callTaskRepository ?? throw new ArgumentNullException(nameof(callTaskRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));

			CarSelectorFactory = (carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory)))
				.CreateCarAutocompleteSelectorFactory();
			LogisticianSelectorFactory = (employeeFactory ?? throw new ArgumentNullException(nameof(employeeFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();
			DriverSelectorFactory = (employeeFactory ?? throw new ArgumentNullException(nameof(employeeFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
			ForwarderSelectorFactory = (employeeFactory ?? throw new ArgumentNullException(nameof(employeeFactory)))
				.CreateWorkingForwarderEmployeeAutocompleteSelectorFactory();

			DeliveryShifts =
				(deliveryShiftRepository ?? throw new ArgumentNullException(nameof(deliveryShiftRepository))).ActiveShifts(UoW);

			RouteListItems = GenerateRouteListItems();

			ConfigureValidationContext();

			ConfigureAndCheckPermissions();
		}

		#region Private methods

		private void ConfigureAndCheckPermissions()
		{
			var currentPermissionService = CommonServices.CurrentPermissionService;
			var canConfirmMileage = currentPermissionService.ValidatePresetPermission("can_confirm_mileage_for_our_GAZelles_Larguses");
			var canUpdate = currentPermissionService.ValidatePresetPermission("logistican") && PermissionResult.CanUpdate;

			CanEdit = (canUpdate && canConfirmMileage)
					  || !(Entity.GetCarVersion.IsCompanyCar &&
						   new[] { CarTypeOfUse.GAZelle, CarTypeOfUse.Largus }.Contains(Entity.Car.CarModel.CarTypeOfUse));

			if(!CanEdit)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не достаточно прав. Обратитесь к руководителю.");
			}

			if(IsEditAvailable)
			{
				if(!Entity.RecountMileage())
				{
					FailInitialize = true;
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

		#endregion

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

		#region Properties

		public IEntityAutocompleteSelectorFactory CarSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory LogisticianSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; }

		public bool CanEdit
		{
			get => _canEdit;
			set
			{
				SetField(ref _canEdit, value);
				OnPropertyChanged(nameof(IsEditAvailable));
				OnPropertyChanged(nameof(IsAcceptAvailable));
			}
		}

		public bool IsEditAvailable => CanEdit && Entity.Status != RouteListStatus.Closed;
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

		#endregion

		#region Commands

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

					SaveAndClose();
				},
				() => true
			));

		public DelegateCommand OpenMapCommand =>
			_openMapCommand ?? (_openMapCommand = new DelegateCommand(() =>
				{
					_gtkTabsOpener.OpenTrackOnMapWnd(Entity.Id);
				},
				() => true
			));

		public DelegateCommand FromTrackCommand =>
			_fromTrackCommand ?? (_fromTrackCommand = new DelegateCommand(() =>
				{
					var track = _trackRepository.GetTrackByRouteListId(UoW, Entity.Id);
					if(track == null)
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
							"Невозможно расчитать растояние, так как в маршрутном листе нет трека", "");
						return;
					}

					Entity.ConfirmedDistance = (decimal)track.TotalDistance.Value;
				},
				() => true
			));

		public DelegateCommand AcceptFineCommand =>
			_acceptFineCommand ?? (_acceptFineCommand = new DelegateCommand(() =>
				{
					var fineReason = "Перерасход топлива";
					_gtkTabsOpener.OpenFineDlg(this, fineReason, Entity);
				},
				() => true
			));

		public DelegateCommand DistributeMiliageCommand =>
			_distributeMiliageCommand ?? (_distributeMiliageCommand = new DelegateCommand(() =>
				{
					var mileageDistributionPage = NavigationManager.OpenViewModel<RouteListMileageDistributionViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.Id), OpenPageOptions.AsSlave);

					mileageDistributionPage.PageClosed += (s, e) =>
					{
						UoW.Session.Refresh(Entity);
					};
				},
				() => true
			));

		#endregion
	}
}
