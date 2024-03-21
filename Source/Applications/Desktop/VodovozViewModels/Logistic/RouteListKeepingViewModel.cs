using Autofac;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
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
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Roboats;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz
{
	public partial class RouteListKeepingViewModel : EntityTabViewModelBase<RouteList>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private ILifetimeScope _lifetimeScope;
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private IEmployeeRepository _employeeRepository;
		private IDeliveryShiftRepository _deliveryShiftRepository;
		private IRouteListProfitabilityController _routeListProfitabilityController;
		private IWageParameterService _wageParameterService;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly IGeneralSettings _generalSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IFileDialogService _fileDialogService;
		private readonly IPermissionResult _permissionResult;

		private Employee _previousForwarder = null;

		private readonly ViewModelEEVMBuilder<Car> _carViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _driverViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _forwarderViewModelEEVMBuilder;
		private List<RouteListKeepingItemNode> _items;
		private RouteListKeepingItemNode _selectedItem;
		private bool _canClose = true;

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
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IRoboatsSettings roboatsSettings,
			IGeneralSettings generalSettings,
			ICallTaskWorker callTaskWorker,
			IFileDialogService fileDialogService,
			DeliveryFreeBalanceViewModel deliveryFreeBalanceViewModel,
			ViewModelEEVMBuilder<Car> carViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> forwarderViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_deliveryShiftRepository = deliveryShiftRepository ?? throw new ArgumentNullException(nameof(deliveryShiftRepository));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			DeliveryFreeBalanceViewModel = deliveryFreeBalanceViewModel ?? throw new ArgumentNullException(nameof(deliveryFreeBalanceViewModel));
			_carViewModelEEVMBuilder = carViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(carViewModelEEVMBuilder));
			_driverViewModelEEVMBuilder = driverViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(driverViewModelEEVMBuilder));
			_forwarderViewModelEEVMBuilder = forwarderViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(forwarderViewModelEEVMBuilder));

			TabName = $"Ведение МЛ №{Entity.Id}";

			_permissionResult = _currentPermissionService.ValidateEntityPermission(typeof(RouteList));
			AllEditing = Entity.Status == RouteListStatus.EnRoute && _permissionResult.CanUpdate;
			IsUserLogist = _currentPermissionService.ValidatePresetPermission(Permissions.Logistic.IsLogistician);
			IsOrderWaitUntilActive = _generalSettings.GetIsOrderWaitUntilActive;
			LogisticanEditing = IsUserLogist && AllEditing;

			CarViewModel = BuildCarEntryViewModel();
			DriverViewModel = BuildDriverEntryViewModel();
			ForwarderViewModel = BuildForwarderEntryViewModel();

			Initialize();
		}

		public virtual ICallTaskWorker CallTaskWorker { get; private set; }
		public RouteListKeepingItemNode[] SelectedRouteListAddresses { get; private set; }
		public string BottlesInfo { get; private set; }
		public GenericObservableList<RouteListKeepingItemNode> Items { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }
		public IEntityEntryViewModel DriverViewModel { get; private set; }
		public IEntityEntryViewModel ForwarderViewModel { get; private set; }
		public DeliveryFreeBalanceViewModel DeliveryFreeBalanceViewModel { get; }

		//2 уровня доступа к виджетам, для всех и для логистов.
		public bool LogisticanEditing { get; }
		public bool IsUserLogist { get; }
		public bool AllEditing { get; }

		public bool IsOrderWaitUntilActive { get; }

		public bool CanChangeForwarder => LogisticanEditing && Entity.CanAddForwarder;
		public IList<DeliveryShift> ActiveShifts => _deliveryShiftRepository.ActiveShifts(UoW);
		public bool AskSaveOnClose => _permissionResult.CanUpdate;

		public override bool HasChanges
		{
			get
			{
				if(_items.All(x => x.Status != RouteListItemStatus.EnRoute))
				{
					return true; //Хак, чтобы вылезало уведомление о закрытии маршрутного листа, даже если ничего не меняли.
				}

				return base.HasChanges;
			}
		}

		private void Initialize()
		{
			Entity.ObservableAddresses.ElementAdded += ObservableAddresses_ElementAdded;
			Entity.ObservableAddresses.ElementRemoved += ObservableAddresses_ElementRemoved;
			Entity.ObservableAddresses.ElementChanged += ObservableAddresses_ElementChanged;

			//evmeForwarder.Changed += ReferenceForwarder_Changed;

			//var employeeFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager);
			//evmeLogistician.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			//evmeLogistician.Binding.AddBinding(Entity, rl => rl.Logistician, widget => widget.Subject).InitializeFromSource();
			//evmeLogistician.Sensitive = LogisticanEditing;

			//speccomboShift.ItemsList = _deliveryShiftRepository.ActiveShifts(UoW);
			//speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			//speccomboShift.Sensitive = LogisticanEditing;

			//datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			//datePickerDate.Sensitive = LogisticanEditing;

			//ylabelLastTimeCall.Binding.AddFuncBinding(Entity, e => GetLastCallTime(e.LastCallTime), w => w.LabelProp).InitializeFromSource();
			//yspinActualDistance.Sensitive = AllEditing;

			//buttonMadeCall.Sensitive = AllEditing;

			//buttonRetriveEnRoute.Sensitive = Entity.Status == RouteListStatus.OnClosing && IsUserLogist
			//	&& _currentPermissionService.ValidatePresetPermission("can_retrieve_routelist_en_route");

			//phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = Startup.MainWin.MangoManager;
			//phoneLogistican.Binding.AddBinding(Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			//phoneDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			//phoneForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			//Заполняем информацию о бутылях
			UpdateBottlesSummaryInfo();

			UpdateNodes();
		}

		public void SelectOrdersById(int[] selectedOrderIds)
		{
			SelectedRouteListAddresses = _items
				.Where(x => selectedOrderIds.Contains(x.RouteListItem.Order.Id))
				.ToArray();
		}

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

			viewModel.Changed += OnEvmeDriverChanged;

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

			viewModel.Changed += OnReferenceForwarderChanged;

			return viewModel;
		}

		private void OnEvmeDriverChanged(object sender, EventArgs e)
		{
			if(Entity.Driver != null)
			{
				if(!Entity.IsDriversDebtInPermittedRangeVerification())
				{
					Entity.Driver = null;
				}
			}
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

		private void ObservableAddresses_ElementAdded(object aList, int[] aIdx)
		{
			UpdateBottlesSummaryInfo();
		}

		private void ObservableAddresses_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			UpdateBottlesSummaryInfo();
		}

		private void ObservableAddresses_ElementChanged(object aList, int[] aIdx)
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
			_items = new List<RouteListKeepingItemNode>();

			foreach(var item in Entity.Addresses.Where(x => x != null))
			{
				_items.Add(new RouteListKeepingItemNode { RouteListItem = item });

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

			_items.ForEach(i => i.StatusChanged += RLI_StatusChanged);

			Items = new GenericObservableList<RouteListKeepingItemNode>(_items);
		}

		private void RLI_StatusChanged(object sender, StatusChangedEventArgs e)
		{
			var newStatus = e.NewStatus;

			if(sender is RouteListKeepingItemNode rli)
			{
				if(newStatus == RouteListItemStatus.Canceled || newStatus == RouteListItemStatus.Overdue)
				{
					//UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(rli.RouteListItem.Order, rli.RouteListItem.RouteList.UoW);
					//TabParent.AddSlaveTab(this, dlg);

					//dlg.DlgSaved += (s, ea) =>
					//{
					//	rli.UpdateStatus(newStatus, CallTaskWorker);
					//	UoW.Save(rli.RouteListItem);
					//	UoW.Commit();
					//};

					//return;
				}

				var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();

				var validationContext = new ValidationContext(Entity, null, new Dictionary<object, object>
				{
					{ "uowFactory", uowFactory }
				});

				var canCreateSeveralOrdersValidationResult =
					rli.RouteListItem.Order.ValidateCanCreateSeveralOrderForDateAndDeliveryPoint(validationContext);

				if(canCreateSeveralOrdersValidationResult != ValidationResult.Success)
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Warning,
						$"Нельзя перевести адрес в статус \"{newStatus.GetEnumTitle()}\": {canCreateSeveralOrdersValidationResult.ErrorMessage} ");

					return;
				}

				rli.UpdateStatus(newStatus, CallTaskWorker);
			}
		}

		public void OnSelectionChanged(object sender, EventArgs args)
		{
			//buttonSetStatusComplete.Sensitive = ytreeviewAddresses.GetSelectedObjects().Any() && AllEditing;
			//buttonChangeDeliveryTime.Sensitive =
			//	ytreeviewAddresses.GetSelectedObjects().Count() == 1
			//	&& _currentPermissionService.ValidatePresetPermission("logistic_changedeliverytime")
			//	&& AllEditing;
		}

		private void OnReferenceForwarderChanged(object sender, EventArgs e)
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
			if(!_canClose)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Дождитесь завершения работы задачи и повторите");
			}

			return _canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			//_canClose = isSensetive;
			//buttonSave.Sensitive = isSensetive;
			//buttonCancel.Sensitive = isSensetive;
		}

		//public override bool Save()
		//{
		//	try
		//	{
		//		SetSensetivity(false);

		//		Entity.CalculateWages(_wageParameterService);

		//		UoWGeneric.Save();

		//		_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
		//		UoW.Save(Entity.RouteListProfitability);
		//		UoW.Commit();

		//		var changedList = _items.Where(item => item.ChangedDeliverySchedule || item.HasChanged).ToList();

		//		if(changedList.Count == 0)
		//		{
		//			return true;
		//		}

		//		var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoWGeneric);

		//		if(currentEmployee == null)
		//		{
		//			_interactiveService.ShowMessage(ImportanceLevel.Info, "Ваш пользователь не привязан к сотруднику, уведомления об изменениях в маршрутном листе не будут отправлены водителю.");
		//			return true;
		//		}

		//		return true;
		//	}
		//	finally
		//	{
		//		SetSensetivity(true);
		//	}
		//}

		#endregion

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			bool hasChanges = _items.Any(item => item.HasChanged);

			if(!hasChanges || _interactiveService.Question("Вы действительно хотите обновить список заказов? Внесенные изменения будут утрачены."))
			{
				UoWGeneric.Session.Refresh(Entity);
				UpdateNodes();
			}
		}

		protected void OnButtonChangeDeliveryTimeClicked(object sender, EventArgs e)
		{
			if(_currentPermissionService.ValidatePresetPermission("logistic_changedeliverytime"))
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
						var selectedResult = args.SelectedNodes.First() as DeliveryScheduleJournalNode;
						if(selectedResult == null)
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

		protected void OnButtonSetStatusCompleteClicked(object sender, EventArgs e)
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

		protected void OnButtonNewFineClicked(object sender, EventArgs e)
		{
			var page = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

			page.ViewModel.SetRouteListById(Entity.Id);
		}

		protected void OnButtonMadeCallClicked(object sender, EventArgs e)
		{
			Entity.LastCallTime = DateTime.Now;
		}

		protected void OnButtonRetriveEnRouteClicked(object sender, EventArgs e)
		{
			Entity.RollBackEnRouteStatus();
		}

		protected void OnBtnReDeliverClicked(object sender, EventArgs e)
		{
			Entity.UpdateStatus(isIgnoreAdditionalLoadingDocument: true);
		}
	}
}
