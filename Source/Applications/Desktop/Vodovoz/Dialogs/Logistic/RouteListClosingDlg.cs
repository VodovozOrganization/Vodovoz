using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.NotifyChange;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Debug;
using QS.Utilities.Extensions;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Infrastructure;
using Vodovoz.Models;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Settings.Cash;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Fuel;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Interactive.YesNoCancelQuestion;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;
using VodovozBusiness.Services.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz
{
	public partial class RouteListClosingDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>, IAskSaveOnCloseViewModel
	{
		#region поля
		private ILifetimeScope _lifetimeScope;

		private ILogger<RouteListClosingDlg> _logger;

		private IOrderSettings _orderSettings;
		private IDeliveryRulesSettings _deliveryRulesSettings;
		private INomenclatureSettings _nomenclatureSettings;
		private IRouteListRepository _routeListRepository;
		private INomenclatureRepository _nomenclatureRepository;

		private IEmployeeRepository _employeeRepository;
		private IDeliveryShiftRepository _deliveryShiftRepository;
		private ICashRepository _cashRepository;
		private ICategoryRepository _categoryRepository;
		private IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private IAccountableDebtsRepository _accountableDebtsRepository;
		private ISubdivisionRepository _subdivisionRepository;
		private ITrackRepository _trackRepository;
		private IFuelRepository _fuelRepository;
		private IRouteListItemRepository _routeListItemRepository;
		private IValidationContextFactory _validationContextFactory;
		private IRouteListProfitabilityController _routeListProfitabilityController;
		private IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;
		private IOrderDiscountsController _orderDiscountsController;
		private IWageParameterService _wageParameterService;
		private IPaymentFromBankClientController _paymentFromBankClientController;
		private IEmployeeNomenclatureMovementRepository _employeeNomenclatureMovementRepository;
		private INewDriverAdvanceSettings _newDriverAdvanceSettings;
		private IPermissionRepository _permissionRepository;
		private IFlyerRepository _flyerRepository;
		private IOrderContractUpdater _contractUpdater;

		private readonly bool _isOpenFromCash;
		private readonly bool _isRoleCashier = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier);
		private bool _canEditCar;
		private bool _canEditDriver;
		private bool _canEditExpeditor;
		private bool _canCreateRouteListWithoutOrders;
		
		private Track track = null;
		private decimal balanceBeforeOp = default(decimal);
		private bool canCloseRoutelist = false;
		private Employee previousForwarder = null;
		private bool _canEdit;
		private bool? _canEditFuelCardNumber;
		

		private bool _needToSelectTerminalCondition = false;
		private bool _hasAccessToDriverTerminal = false;

		List<ReturnsNode> allReturnsToWarehouse;
		private IEnumerable<DefectSource> defectiveReasons;
		int bottlesReturnedToWarehouse;
		int bottlesReturnedTotal;
		int defectiveBottlesReturnedToWarehouse;

		private Dictionary<int, HashSet<RouteListAddressKeepingDocumentItem>> _addressKeepingDocumentItemsCacheList =
			new Dictionary<int, HashSet<RouteListAddressKeepingDocumentItem>>();

		private Dictionary<int, HashSet<RouteListAddressKeepingDocumentItem>> _addressKeepingDocumentBottlesCacheList =
			new Dictionary<int, HashSet<RouteListAddressKeepingDocumentItem>>();

		private List<int> _ignoreReceiptsForOrderIds = new List<int>();

		public virtual ICallTaskWorker CallTaskWorker { get; private set; }

		public ITdiCompatibilityNavigation NavigationManager => Startup.MainWin.NavigationManager;

		#endregion

		#region Конструкторы и конфигурирование диалога

		public RouteListClosingDlg(RouteList routeList) : this(routeList.Id) { }

		public RouteListClosingDlg(int routeListId, bool isOpenFromCash = false)
		{
			RetrieveDependencies();
			_isOpenFromCash = isOpenFromCash;
			Build();

			PerformanceHelper.StartMeasurement();

			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<RouteList>(routeListId);

			TabName = string.Format("Закрытие маршрутного листа №{0}", Entity.Id);
			PerformanceHelper.AddTimePoint("Создан UoW");
			ConfigureDlg();
		}

		public bool AskSaveOnClose => _canEdit;

		private void RetrieveDependencies()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

			_logger = _lifetimeScope.Resolve<ILogger<RouteListClosingDlg>>();

			_currentPermissionService = _lifetimeScope.Resolve<ICurrentPermissionService>();
			_counterpartyService = _lifetimeScope.Resolve<ICounterpartyService>();
			_interactiveService = _lifetimeScope.Resolve<IInteractiveService>();
			_employeeService = _lifetimeScope.Resolve<IEmployeeService>();
			_userRepository = _lifetimeScope.Resolve<IUserRepository>();
			_orderRepository = _lifetimeScope.Resolve<IOrderRepository>();
			_discountReasonRepository = _lifetimeScope.Resolve<IDiscountReasonRepository>();
			_nomenclatureOnlineSettings = _lifetimeScope.Resolve<INomenclatureOnlineSettings>();

			_orderSettings = _lifetimeScope.Resolve<IOrderSettings>();
			_deliveryRulesSettings = _lifetimeScope.Resolve<IDeliveryRulesSettings>();
			_nomenclatureSettings = _lifetimeScope.Resolve<INomenclatureSettings>();
			_routeListRepository = _lifetimeScope.Resolve<IRouteListRepository>();
			_nomenclatureRepository = _lifetimeScope.Resolve<INomenclatureRepository>();

			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_deliveryShiftRepository = _lifetimeScope.Resolve<IDeliveryShiftRepository>();
			_cashRepository = _lifetimeScope.Resolve<ICashRepository>();
			_categoryRepository = _lifetimeScope.Resolve<ICategoryRepository>();
			_financialCategoriesGroupsSettings = _lifetimeScope.Resolve<IFinancialCategoriesGroupsSettings>();
			_accountableDebtsRepository = _lifetimeScope.Resolve<IAccountableDebtsRepository>();
			_subdivisionRepository = _lifetimeScope.Resolve<ISubdivisionRepository>();
			_trackRepository = _lifetimeScope.Resolve<ITrackRepository>();
			_fuelRepository = _lifetimeScope.Resolve<IFuelRepository>();
			_routeListItemRepository = _lifetimeScope.Resolve<IRouteListItemRepository>();
			_validationContextFactory = _lifetimeScope.Resolve<IValidationContextFactory>();
			_routeListProfitabilityController = _lifetimeScope.Resolve<IRouteListProfitabilityController>();
			_routeListAddressKeepingDocumentController = _lifetimeScope.Resolve<IRouteListAddressKeepingDocumentController>();
			_orderDiscountsController = _lifetimeScope.Resolve<IOrderDiscountsController>();

			_wageParameterService = _lifetimeScope.Resolve<IWageParameterService>();
			_paymentFromBankClientController = _lifetimeScope.Resolve<IPaymentFromBankClientController>();
			_employeeNomenclatureMovementRepository = _lifetimeScope.Resolve<IEmployeeNomenclatureMovementRepository>();
			_newDriverAdvanceSettings = _lifetimeScope.Resolve<INewDriverAdvanceSettings>();

			_permissionRepository = _lifetimeScope.Resolve<IPermissionRepository>();

			CallTaskWorker = _lifetimeScope.Resolve<ICallTaskWorker>();

			_flyerRepository = _lifetimeScope.Resolve<IFlyerRepository>();
			_contractUpdater = _lifetimeScope.Resolve<IOrderContractUpdater>();
		}

		private void ConfigureDlg()
		{
			var availableStatusesForAccepting = new RouteListStatus[]
			{
				RouteListStatus.Delivered,
				RouteListStatus.OnClosing
			};

			_canEdit = _isRoleCashier
				&& permissionResult.CanUpdate
				&& (!Entity.WasAcceptedByCashier || availableStatusesForAccepting.Contains(Entity.Status));

			if(Entity.AddressesOrderWasChangedAfterPrinted) {
				MessageDialogHelper.RunInfoDialog("<span color=\"red\">ВНИМАНИЕ!</span> Порядок адресов в Мл был изменен!");
			}

			permissioncommentview.UoW = UoW;
			permissioncommentview.Title = "Комментарий по проверке закрытия МЛ: ";
			permissioncommentview.Comment = Entity.CashierReviewComment;
			permissioncommentview.PermissionName = "can_edit_cashier_review_comment";
			permissioncommentview.Comment = Entity.CashierReviewComment;
			permissioncommentview.CommentChanged += (comment) => { 
				Entity.CashierReviewComment = comment;
				HasChanges = true;
			};

			canCloseRoutelist = _permissionRepository
				.HasAccessToClosingRoutelist(UoW, _subdivisionRepository, _employeeRepository, ServicesConfig.UserService) && _canEdit;
			Entity.ObservableFuelDocuments.ElementAdded += ObservableFuelDocuments_ElementAdded;
			Entity.ObservableFuelDocuments.ElementRemoved += ObservableFuelDocuments_ElementRemoved;

			var isEditableStatus = !(Entity.Status == RouteListStatus.Delivered
			                         || Entity.Status == RouteListStatus.OnClosing
			                         || Entity.Status == RouteListStatus.MileageCheck
			                         || Entity.Status == RouteListStatus.Closed);
			_canEditCar = _canEdit 
			              && (isEditableStatus 
			              || ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.RouteListPermissions.CanEditCarOnCloseRouteList));
			entityentryCar.ViewModel = BuildCarEntryViewModel();
			entityentryCar.Sensitive = _canEditCar;
			
			var employeeJournalFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			
			_canEditDriver = _canEdit
			                && (isEditableStatus
			                || ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.RouteListPermissions.CanEditDriverOnCloseRouteList));
			
			evmeDriver.SetEntityAutocompleteSelectorFactory(
				employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory(true));
			evmeDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			evmeDriver.Changed += OnEvmeDriverChanged;
			evmeDriver.Sensitive = _canEditDriver;
			
			_canEditExpeditor =  _canEdit 
			                     && (isEditableStatus
			                     || ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.RouteListPermissions.CanEditExpeditorOnCloseRouteList));
			
			_canCreateRouteListWithoutOrders = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders);
			
			previousForwarder = Entity.Forwarder;
			
			evmeForwarder.SetEntityAutocompleteSelectorFactory(
				employeeJournalFactory.CreateWorkingForwarderEmployeeAutocompleteSelectorFactory(true));
			evmeForwarder.Binding.AddSource(Entity)
				.AddBinding(rl => rl.Forwarder, widget => widget.Subject)
				.AddFuncBinding(rl => rl.CanAddForwarder && _canEdit, widget => widget.Sensitive)
				.InitializeFromSource();
			evmeForwarder.Changed += ReferenceForwarder_Changed;
			evmeForwarder.Sensitive = _canEditExpeditor;
			
			evmeLogistician.SetEntityAutocompleteSelectorFactory(employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeLogistician.Binding.AddBinding(Entity, rl => rl.Logistician, widget => widget.Subject).InitializeFromSource();

			speccomboShift.ItemsList = _deliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();

			ycheckConfirmDifferences.Binding.AddBinding(Entity, e => e.DifferencesConfirmed, w => w.Active).InitializeFromSource();

			ytextClosingComment.Binding.AddBinding(Entity, e => e.ClosingComment, w => w.Buffer.Text).InitializeFromSource();
			labelOrderEarly.Text = "Сдано ранее: " + GetCashOrder().ToShortCurrencyString();
			spinCashOrder.Value = 0;
			advanceSpinbutton.Value = 0;
			advanceSpinbutton.Visible = false;

			PerformanceHelper.AddTimePoint("Создан диалог");

			_routeListAddressKeepingDocumentController =
				new RouteListAddressKeepingDocumentController(_employeeRepository, _nomenclatureRepository);

			PerformanceHelper.AddTimePoint("Предварительная загрузка");

			routeListAddressesView.UoW = UoW;
			routeListAddressesView.RouteList = Entity;
			foreach(RouteListItem item in routeListAddressesView.Items) 
			{
				item.Order.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged;
				item.Order.ObservableOrderItems.ElementAdded += ObservableOrderItems_ElementAdded;
				item.Order.ObservableOrderItems.ElementRemoved += ObservableOrderItems_ElementRemoved;

				item.Order.ObservableOrderEquipments.ElementChanged += ObservableOrderItems_ElementChanged;
				item.Order.ObservableOrderEquipments.ElementAdded += ObservableOrderItems_ElementAdded;
				item.Order.ObservableOrderEquipments.ElementRemoved += ObservableOrderItems_ElementRemoved;

				item.Order.ObservableOrderDepositItems.ElementChanged += ObservableOrderItems_ElementChanged;
				item.Order.ObservableOrderDepositItems.ElementAdded += ObservableOrderItems_ElementAdded;
				item.Order.ObservableOrderDepositItems.ElementRemoved += ObservableOrderItems_ElementRemoved;
			}
			routeListAddressesView.Items.ElementChanged += OnRouteListItemChanged;
			routeListAddressesView.OnClosingItemActivated += OnRouteListItemActivated;
			routeListAddressesView.BottlesReturnedEdited += OnBottlesReturnedEdited;
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
			PerformanceHelper.AddTimePoint("заполнили список адресов");
			ReloadReturnedToWarehouse();
			var returnableOrderItems = routeListAddressesView.Items.Where(address => address.IsDelivered())
																   .SelectMany(address => address.Order.OrderItems)
																   .Where(orderItem => !orderItem.Nomenclature.IsSerial)
																   .Where(orderItem => Nomenclature.GetCategoriesForShipment().Any(nom => nom == orderItem.Nomenclature.Category));

			routelistdiscrepancyview.RouteList = Entity;
			routelistdiscrepancyview.FindDiscrepancies();
			routelistdiscrepancyview.FineChanged += Routelistdiscrepancyview_FineChanged;

			PerformanceHelper.AddTimePoint("Получили возврат на склад");
			//FIXME Убрать из этого места первоначальное заполнение. Сейчас оно вызывается при переводе статуса на сдачу. После того как не нормально не переведенных в закрытие маршрутников, тут заполение можно убрать.
			if(!Entity.ClosingFilled)
				Entity.FirstFillClosing(_wageParameterService);

			PerformanceHelper.AddTimePoint("Закончено первоначальное заполнение");

			rightsidepanel1.Panel = vboxHidenPanel;
			rightsidepanel1.IsHided = true;

			xpndRouteListInfo.Expanded = Entity.Status == RouteListStatus.Closed;

			PerformanceHelper.AddTimePoint("Заполнили расхождения");

			ytreeviewFuelDocuments.ItemsDataSource = Entity.ObservableFuelDocuments;
			ytreeviewFuelDocuments.Reorderable = true;
			Entity.ObservableFuelDocuments.ListChanged += ObservableFuelDocuments_ListChanged;
			UpdateFuelDocumentsColumns();

			enummenuRLActions.ItemsEnum = typeof(RouteListActions);
			enummenuRLActions.EnumItemClicked += EnummenuRLActions_EnumItemClicked;

			CheckWage();

			LoadDataFromFine();
			OnItemsUpdated();
			PerformanceHelper.AddTimePoint("Загрузка штрафов");
			GetFuelInfo();
			UpdateFuelInfo();
			PerformanceHelper.AddTimePoint("Загрузка бензина");

			PerformanceHelper.Main.PrintAllPoints(_logger);

			//Подписки на обновления
			OrmMain.GetObjectDescription<CarUnloadDocument>().ObjectUpdatedGeneric += OnCalUnloadUpdated;

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<Expense>(s => CalculateTotal());
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<Income>(s => CalculateTotal());

			enumPrint.ItemsEnum = typeof(RouteListPrintDocuments);
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintDocuments)e.ItemEnum);

			Entity.PropertyChanged += Entity_PropertyChanged;
			
			foreach (RouteListItem routeListItem in routeListAddressesView.Items)
				routeListItem.RecalculateTotalCash();
			CalculateTotal();

			UpdateSensitivity();

			notebook1.ShowTabs = false;
			notebook1.Page = 0;

			_hasAccessToDriverTerminal = _canEdit;
			var baseDoc = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, Entity.Driver);
			_needToSelectTerminalCondition = baseDoc is DriverAttachedTerminalGiveoutDocument && baseDoc.CreationDate.Date <= Entity?.Date;
			hboxTerminalCondition.Visible = _hasAccessToDriverTerminal && _needToSelectTerminalCondition;

			enumTerminalCondition.ItemsEnum = typeof(DriverTerminalCondition);
			enumTerminalCondition.Binding
				.AddBinding(Entity, e => e.DriverTerminalCondition, w => w.SelectedItemOrNull).InitializeFromSource();

			var deliveryFreeBalanceViewModel = new DeliveryFreeBalanceViewModel();
			var deliveryfreebalanceview = new DeliveryFreeBalanceView(deliveryFreeBalanceViewModel);
			deliveryfreebalanceview.Binding
				.AddBinding(Entity, e => e.ObservableDeliveryFreeBalanceOperations, w => w.ObservableDeliveryFreeBalanceOperations)
				.InitializeFromSource();
			deliveryfreebalanceview.ShowAll();
			yhboxDeliveryFreeBalance.PackStart(deliveryfreebalanceview, true, true, 0);

			ybuttonCashChangeReturn.Clicked += OnYbuttonCashChangeReturnClicked;

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		private void OnBottlesReturnedEdited(object sender, int bottlesReturned)
		{
			var node = routeListAddressesView.GetSelectedRouteListItem();
			node.BottlesReturned = bottlesReturned;
			CreateBottlesReturnFreeBalanceOperations(node);
		}

		private void CreateBottlesReturnFreeBalanceOperations(RouteListItem node)
		{
			if(!_addressKeepingDocumentBottlesCacheList.ContainsKey(node.Id))
			{
				_addressKeepingDocumentBottlesCacheList.Add(node.Id, new HashSet<RouteListAddressKeepingDocumentItem>());
			}

			_addressKeepingDocumentBottlesCacheList[node.Id] = _routeListAddressKeepingDocumentController
				.CreateOrUpdateRouteListKeepingDocumentByDiscrepancy(UoW, ServicesConfig.UnitOfWorkFactory, node, _addressKeepingDocumentBottlesCacheList[node.Id], true);

			ReloadDiscrepancies();
		}

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var viewModel = new LegacyEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Car)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.UseViewModelDialog<CarViewModel>()
				.Finish();

			viewModel.CanViewEntity = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = Entity.Id.ToString();
			}
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

		private void OnYbuttonCashChangeReturnClicked(object sender, EventArgs e)
		{
			var page = NavigationManager.OpenViewModel<IncomeViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());
			page.ViewModel.ConFigureForReturnChange(Entity.Id);
		}

		private void UpdateYbuttonCashChangeReturnSensitivity()
		{
			var hasUnclosedAdvances = GetRouteListCashExpenses() > GetRouteListCashReturn();
			var routeListStatusesForCloseAdvance = 
				new[] { RouteListStatus.Delivered, RouteListStatus.OnClosing, RouteListStatus.MileageCheck };
			var hasStatusForCloseAdvance = routeListStatusesForCloseAdvance.Contains(Entity.Status);

			ybuttonCashChangeReturn.Sensitive = hasUnclosedAdvances && hasStatusForCloseAdvance;
		}

		private void UpdateSensitivity()
		{
			toggleWageDetails.Sensitive = Entity.Car.CarModel.CarTypeOfUse != CarTypeOfUse.Truck;

			var sensStatuses = new[] { RouteListStatus.OnClosing, RouteListStatus.MileageCheck, RouteListStatus.Delivered};
			if(!sensStatuses.Contains(Entity.Status))
			{
				ytextviewFuelInfo.Sensitive = false;
				ycheckHideCells.Sensitive = false;
				vbxFuelTickets.Sensitive = false;
				speccomboShift.Sensitive = false;
				evmeLogistician.Sensitive = false;
				datePickerDate.Sensitive = false;
				hbox11.Sensitive = false;
				routelistdiscrepancyview.Sensitive = false;
				hbxStatistics1.Sensitive = false;
				hbxStatistics2.Sensitive = false;
				hbxStatistics3.Sensitive = false;
				enummenuRLActions.Sensitive = false;
				permissioncommentview.Sensitive = _isRoleCashier && permissionResult.CanUpdate;
				buttonSave.Sensitive = _canEdit;

				HasChanges = false;

				return;
			}

			speccomboShift.Sensitive = false;
			vbxFuelTickets.Sensitive = CheckIfCashier();
			evmeLogistician.Sensitive = _canEdit;
			datePickerDate.Sensitive = _canEdit;
			ycheckConfirmDifferences.Sensitive = _canEdit &&
				(Entity.Status == RouteListStatus.OnClosing || 
				 Entity.Status == RouteListStatus.Delivered);
			ytextClosingComment.Sensitive = _canEdit;
			routeListAddressesView.IsEditing = _canEdit;
			ycheckHideCells.Sensitive = _canEdit;
			routelistdiscrepancyview.Sensitive = _canEdit;
			buttonReturnedRefresh.Sensitive = _canEdit;
			buttonAddFuelDocument.Sensitive = Entity.Car?.FuelType?.Cost != null && Entity.Driver != null && _canEdit;
			buttonDeleteFuelDocument.Sensitive = Entity.Car?.FuelType?.Cost != null && Entity.Driver != null && _canEdit;
			enummenuRLActions.Sensitive = _canEdit;
			advanceCheckbox.Sensitive = advanceSpinbutton.Sensitive = _canEdit;
			spinCashOrder.Sensitive = buttonCreateCashOrder.Sensitive = _canEdit;
			buttonCalculateCash.Sensitive = _canEdit;
			labelWage1.Visible = _canEdit;
			permissioncommentview.Sensitive = _isRoleCashier && permissionResult.CanUpdate;
			buttonSave.Sensitive = _canEdit;
			UpdateButtonState();
		}

		/// <summary>
		/// Перепроверка зарплаты водителя и экспедитора
		/// </summary>
		private void CheckWage()
		{
			decimal driverCurrentWage = Entity.GetDriversTotalWage();
			decimal forwarderCurrentWage = Entity.GetForwardersTotalWage();
			decimal driverRecalcWage = Entity.GetRecalculatedDriverWage(_wageParameterService);
			decimal forwarderRecalcWage = Entity.GetRecalculatedForwarderWage(_wageParameterService);

			string recalcWageMessage = "Найдены расхождения после пересчета зарплаты:";
			bool hasDiscrepancy = false;
			if(driverRecalcWage != driverCurrentWage) {
				recalcWageMessage += string.Format("\nВодителя: до {0}, после {1}", driverCurrentWage, driverRecalcWage);
				hasDiscrepancy = true;
			}
			if(forwarderRecalcWage != forwarderCurrentWage) {
				recalcWageMessage += string.Format("\nЭкспедитора: до {0}, после {1}", forwarderCurrentWage, forwarderRecalcWage);
				hasDiscrepancy = true;
			}
			recalcWageMessage += string.Format("\nПересчитано.");

			if(hasDiscrepancy && Entity.Status == RouteListStatus.Closed) {
				MessageDialogHelper.RunInfoDialog(recalcWageMessage);
				Entity.RecalculateAllWages(_wageParameterService);
			}
		}

		void ObservableFuelDocuments_ListChanged(object aList)
		{
			UpdateFuelDocumentsColumns();
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Car) 
				&& Entity.Car != null 
				&& Entity.GetCarVersion == null)
			{
				MessageDialogHelper.RunErrorDialog(
					$"Ошибка при выборе автомобиля с гос. номером \"{Entity.Car.RegistrationNumber}\". " +
					$"Нет данных о версии автомобиля на выбранную дату доставки {Entity.Date:dd.MM.yyyy}.");
				
				Entity.Car = null;
			}

			if(e.PropertyName == nameof(Entity.Driver))
			{
				if(Entity.Driver == null)
				{
					Entity.Forwarder = null;
				}

				if(Entity.Driver != null && Entity.Driver.GetActualWageParameter(Entity.Date) == null)
				{
					MessageDialogHelper.RunErrorDialog(
						$"Нет данных о параметрах расчета зарплаты водителя \"{Entity.Driver.FullName}\" " +
						$"на выбранную дату доставки {Entity.Date:dd.MM.yyyy}.");

					Entity.Driver = null;
					Entity.Forwarder = null;
				}
			}

			switch(e.PropertyName)
			{
				case nameof(Entity.NormalWage):
				case nameof(Entity.Driver) when Entity.Car != null && Entity.Driver != null:
				case nameof(Entity.Car) when Entity.Car != null && Entity.Driver != null:
					Entity.RecalculateAllWages(_wageParameterService);
					break;
			}
		}

		private void UpdateFuelDocumentsColumns()
		{
			var config = ColumnsConfigFactory.Create<FuelDocument>();

			config.AddColumn("Дата")
					.AddTextRenderer(node => node.Date.ToShortDateString())
				  .AddColumn("Литры")
					.AddNumericRenderer(node => node.FuelOperation.LitersGived)
					.Adjustment(new Adjustment(0, -100000, 100000, 10, 100, 10))
				  .AddColumn("№ ТК")
					.AddTextRenderer(n => n.FuelCardNumber)
					.Editable(CanEditFuelCardNumber)
				  .AddColumn("")
					.AddTextRenderer()
				  .RowCells();

			ytreeviewFuelDocuments.ColumnsConfig = config.Finish();
		}

		protected virtual bool CanEditFuelCardNumber => _canEditFuelCardNumber
			?? (_canEditFuelCardNumber =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanChangeFuelCardNumber)).Value;

		private decimal GetCashOrder() => _cashRepository.GetRouteListBalanceExceptAccountableCash(UoW, Entity.Id);
		private decimal GetRouteListCashExpenses() => _cashRepository.GetRouteListCashExpensesSum(UoW, Entity.Id);
		private decimal GetRouteListCashReturn() => _cashRepository.GetRouteListCashReturnSum(UoW, Entity.Id);
		private decimal GetRouteListAdvanceReport() => _cashRepository.GetRouteListAdvancsReportsSum(UoW, Entity.Id);

		private decimal GetTerminalOrdersSum()
		{
			var result = Entity.Addresses.Where(x => x.Order.PaymentType == PaymentType.Terminal
					&& x.Status != RouteListItemStatus.Transfered)
				.Sum(x => x.Order.OrderSum);

			return result;
		}

		private decimal GetTerminalSbpOrdersSum()
		{
			var result = Entity.Addresses.Where(x => 
					x.Order.PaymentType == PaymentType.Terminal
					&& x.Order.PaymentByTerminalSource == PaymentByTerminalSource.ByQR
					&& x.Status != RouteListItemStatus.Transfered)
				.Sum(x => x.Order.OrderSum);

			return result;
		}

		void OnCalUnloadUpdated(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<CarUnloadDocument> e)
		{
			if(e.UpdatedSubjects.Any(x => x.RouteList.Id == Entity.Id))
				ReloadDiscrepancies();
		}

		#endregion

		#region Методы

		void ReferenceForwarder_Changed(object sender, EventArgs e)
		{
			var newForwarder = Entity.Forwarder;

			if(NoActualWageParameterForSelectedForwarder)
			{
				MessageDialogHelper.RunErrorDialog(
					$"Нет данных о параметрах расчета зарплаты экспедитора \"{Entity.Forwarder.FullName}\" " +
					$"на выбранную дату доставки {Entity.Date:dd.MM.yyyy}.");

				Entity.Forwarder = null;
				newForwarder = null;
			}

			if(Entity.Driver != null)
			{
				if((previousForwarder == null && newForwarder != null)
						|| (previousForwarder != null && newForwarder == null))
				{
					Entity.RecalculateAllWages(_wageParameterService);
				}
			}

			previousForwarder = Entity.Forwarder;
			OnItemsUpdated();
		}

		private bool NoActualWageParameterForSelectedForwarder =>
			Entity.Forwarder != null
			&& Entity.Forwarder.GetActualWageParameter(Entity.Date) == null;

		void EnummenuRLActions_EnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			switch((RouteListActions)e.ItemEnum) {
				case RouteListActions.CreateNewFine:
					var page = NavigationManager.OpenViewModelOnTdi<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.IgnoreHash);

					page.ViewModel.SetRouteListById(Entity.Id);
					break;
				case RouteListActions.TransferReceptionToAnotherRL:
					this.TabParent.AddSlaveTab(
						this, new TransferGoodsBetweenRLDlg(Entity, 
							TransferGoodsBetweenRLDlg.OpenParameter.Sender,
							_employeeNomenclatureMovementRepository)
					);
					break;
				case RouteListActions.TransferReceptionToThisRL:
					this.TabParent.AddSlaveTab(
						this, new TransferGoodsBetweenRLDlg(Entity, 
							TransferGoodsBetweenRLDlg.OpenParameter.Receiver,
							_employeeNomenclatureMovementRepository)
					);
					break;
				case RouteListActions.TransferAddressesToThisRL:
					if(UoW.HasChanges) {
						if(MessageDialogHelper.RunQuestionDialog("Необходимо сохранить документ.\nСохранить?"))
						{
							Save();
						}
						else
						{
							return;
						}
					}
					var transferingRecieverPage = NavigationManager.OpenViewModelOnTdi<RouteListTransferringViewModel>(this);
					transferingRecieverPage.ViewModel.TargetRouteListId = Entity.Id;
					break;
				case RouteListActions.TransferAddressesToAnotherRL:
					if(UoW.HasChanges) {
						if(MessageDialogHelper.RunQuestionDialog("Необходимо сохранить документ.\nСохранить?"))
						{
							Save();
						}
						else
						{
							return;
						}
					}
					var transferingSenderPage = NavigationManager.OpenViewModelOnTdi<RouteListTransferringViewModel>(this);
					transferingSenderPage.ViewModel.SourceRouteListId = Entity.Id;
					break;
				default:
					break;
			}
		}

		void Routelistdiscrepancyview_FineChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		void OnRouteListItemActivated(object sender, RowActivatedArgs args)
		{
			var node = routeListAddressesView.GetSelectedRouteListItem();

			if(node is null)
			{
				MessageDialogHelper.RunErrorDialog("Не выбран адрес. Если ошибка будет повторяться - переоткройте вкладку");
				return;
			}
			
			var dlg = new OrderReturnsView(
				UoW,
				_orderDiscountsController,
				CallTaskWorker,
				_counterpartyService,
				_currentPermissionService,
				_interactiveService,
				_employeeService,
				_userRepository,
				_orderRepository,
				_discountReasonRepository,
				_wageParameterService,
				_orderSettings,
				_nomenclatureOnlineSettings,
				_deliveryRulesSettings,
				_flyerRepository,
				NavigationManager,
				_lifetimeScope,
				_contractUpdater);
			
			dlg.ConfigureForRouteListAddress(node);
			dlg.TabClosed += OnOrderReturnsViewTabClosed;
			TabParent.AddSlaveTab(this, dlg);
		}

		private void OnOrderReturnsViewTabClosed(object sender, EventArgs e)
		{
			if(sender is OrderReturnsView orderReturnsView)
			{
				if(orderReturnsView.IgnoreReceipt)
				{
					if(orderReturnsView.OrderId.HasValue && !_ignoreReceiptsForOrderIds.Contains(orderReturnsView.OrderId.Value))
					{
						_ignoreReceiptsForOrderIds.Add(orderReturnsView.OrderId.Value);
					}
				}
				else
				{
					if(orderReturnsView.OrderId.HasValue && _ignoreReceiptsForOrderIds.Contains(orderReturnsView.OrderId.Value))
					{
						_ignoreReceiptsForOrderIds.Remove(orderReturnsView.OrderId.Value);
					}
				}
			}

			var node = routeListAddressesView.GetSelectedRouteListItem();

			if(!_addressKeepingDocumentItemsCacheList.ContainsKey(node.Id))
			{
				_addressKeepingDocumentItemsCacheList.Add(node.Id, new HashSet<RouteListAddressKeepingDocumentItem>());
			}

			RouteListItemStatus oldAddressStatus;

			using (var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Получение сохранённого статуса адреса"))
			{
				oldAddressStatus = uow.GetById<RouteListItem>(node.Id).Status;
			}

			var isFromRouteListClosingNewUndelivery = RouteListItem.GetUndeliveryStatuses().Contains(node.Status)
				&& !RouteListItem.GetUndeliveryStatuses().Contains(oldAddressStatus);

			_addressKeepingDocumentItemsCacheList[node.Id] = _routeListAddressKeepingDocumentController
				.CreateOrUpdateRouteListKeepingDocumentByDiscrepancy(UoW, ServicesConfig.UnitOfWorkFactory, node, _addressKeepingDocumentItemsCacheList[node.Id],
				isFromRouteListClosingUndelivery: isFromRouteListClosingNewUndelivery);

			CreateBottlesReturnFreeBalanceOperations(node);

			ReloadDiscrepancies();

			((OrderReturnsView)sender).TabClosed -= OnOrderReturnsViewTabClosed;
		}

		void OnRouteListItemChanged(object aList, int[] aIdx)
		{
			var item = routeListAddressesView.Items[aIdx[0]];

			if(NoActualWageParameterForSelectedForwarder)
			{
				Entity.Forwarder = null;
			}
			
			if(Entity.Driver != null)
			{
				Entity.RecalculateWagesForRouteListItem(item, _wageParameterService);
			}

			item.RecalculateTotalCash();
			if(!item.IsDelivered() && item.Status != RouteListItemStatus.Transfered)
				foreach(var itm in item.Order.OrderItems)
					itm.SetActualCountZero();

			routelistdiscrepancyview.FindDiscrepancies();
			OnItemsUpdated();
		}

		void ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			OrderReturnsChanged();
		}

		void ObservableOrderItems_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			OrderReturnsChanged();
		}

		void ObservableOrderItems_ElementChanged(object aList, int[] aIdx)
		{
			OrderReturnsChanged();
		}

		void OrderReturnsChanged()
		{
			foreach(var item in routeListAddressesView.Items) {
				var rli = item as RouteListItem;
				Entity.RecalculateWagesForRouteListItem(rli, _wageParameterService);
				rli.RecalculateTotalCash();
			}
			routelistdiscrepancyview.FindDiscrepancies();
			OnItemsUpdated();
		}

		void OnItemsUpdated()
		{
			CalculateTotal();
			UpdateButtonState();
		}

		void UpdateButtonState()
		{
			buttonAccept.Sensitive = 
				(Entity.Status == RouteListStatus.OnClosing || Entity.Status == RouteListStatus.MileageCheck
															|| Entity.Status == RouteListStatus.Delivered) 
				&& canCloseRoutelist;
		}

		private bool buttonFineEditState;

		/// <summary>
		/// Не использовать это поле напрямую, используйте свойство DefaultBottle
		/// </summary>
		Nomenclature defaultBottle;
		private ICounterpartyService _counterpartyService;
		private ICurrentPermissionService _currentPermissionService;
		private IInteractiveService _interactiveService;
		private IEmployeeService _employeeService;
		private IUserRepository _userRepository;
		private IOrderRepository _orderRepository;
		private IDiscountReasonRepository _discountReasonRepository;
		private INomenclatureOnlineSettings _nomenclatureOnlineSettings;

		Nomenclature DefaultBottle {
			get {
				if(defaultBottle == null) {
					var db = _nomenclatureRepository.GetDefaultBottleNomenclature(UoW);
					defaultBottle = db ?? throw new Exception("Не найдена номенклатура бутыли по умолчанию, указанная в параметрах приложения: default_bottle_nomenclature");
				}
				return defaultBottle;
			}
		}

		void CalculateTotal()
		{
			var items = routeListAddressesView.Items.Where(item => item.IsDelivered()).ToList();
			bottlesReturnedTotal = items.Sum(item => item.BottlesReturned + item.Order.BottlesByStockActualCount);
			int fullBottlesTotal = (int)items.SelectMany(item => item.Order.OrderItems)
										.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && item.Nomenclature.TareVolume == TareVolume.Vol19L)
										.Sum(item => item.ActualCount ?? 0);
			decimal depositsCollectedTotal = items.Sum(item => item.BottleDepositsCollected);
			decimal equipmentDepositsCollectedTotal = items.Sum(item => item.EquipmentDepositsCollected);
			decimal totalCollected = items.Sum(item => item.TotalCash);
			Entity.CalculateWages(_wageParameterService);
			decimal driverWage = Entity.GetDriversTotalWage();
			decimal forwarderWage = Entity.GetForwardersTotalWage();

			var totalCachAmount = totalCollected - Entity.PhoneSum;
			var routeListRevenue = GetCashOrder() - (decimal)advanceSpinbutton.Value;
			var routeListCashAdvance = GetRouteListCashExpenses();
			var routeListCashReturn = GetRouteListCashReturn();
			var routeListAdvancesReturn = GetRouteListAdvanceReport();

			var routeListDebt = Entity.RouteListDebt;
			decimal unclosedAdvanceMoney = default(decimal);

			if(Entity.Driver != null)
			{
				unclosedAdvanceMoney = _routeListRepository.GetUnclosedRouteListsDebtsSumByDriver(UoW, Entity.Driver.Id);
			}

			labelAddressCount.Text = string.Format("Адр.: {0}", Entity.UniqueAddressCount);
			labelPhone.Text = string.Format(
				"Сот. связь: {0} {1}",
				Entity.PhoneSum,
				CurrencyWorks.CurrencyShortName
			);
			labelFullBottles.Text = string.Format("Полных бут.: {0}", fullBottlesTotal);
			labelEmptyBottles.Text = string.Format("Пустых бут.: {0}", bottlesReturnedTotal);
			labelDeposits.Text = string.Format(
				"Из них возврат залогов (информационно): {0}",
				(depositsCollectedTotal + equipmentDepositsCollectedTotal).ToShortCurrencyString()
			);
			labelCash.Text = string.Format(
				"Нал по заказам: {0}",
				totalCollected.ToShortCurrencyString()
			);
			labelTotalCollected.Text = string.Format(
				"Итоговая сумма(нал.): {0}",
				totalCachAmount.ToShortCurrencyString()
			);
			labelTerminalSum.Text = $"По терминалу: {GetTerminalOrdersSum().ToShortCurrencyString()}";
			labelTerminalIncludedSBP.Text = $"В том числе по СБП: {GetTerminalSbpOrdersSum().ToShortCurrencyString()}";
			labelTotal.Markup = string.Format(
				"Сдано выручка по МЛ: {0}",
				routeListRevenue.ToShortCurrencyString()
			);
			labelWage1.Markup = string.Format(
				"ЗП вод.: <b>{0}</b> {2}" + "  " + "ЗП эксп.: <b>{1}</b> {2}",
				driverWage,
				forwarderWage,
				CurrencyWorks.CurrencyShortName
			);
			labelEmptyBottlesFommula.Markup = string.Format("Тара: <b>{0}</b><sub>(выгружено на склад)</sub> - <b>{1}</b><sub>(по документам)</sub> =",
				bottlesReturnedToWarehouse,
				bottlesReturnedTotal
			);
			labelGivenChange.Markup = $"Выдано по МЛ (сдача): {routeListCashAdvance.ToShortCurrencyString()}";
			labelReceivedChange.Markup = $"Сдано сдача по МЛ: {(routeListCashReturn + routeListAdvancesReturn).ToShortCurrencyString()}";
			labelRouteListDebt.Markup = $"Долг по МЛ: <b>{routeListDebt.ToShortCurrencyString()}</b>";			

			ylabelUnclosedAdvancesMoney.Markup =
				unclosedAdvanceMoney > 0m
				? $"<span foreground='{GdkColors.DangerText.ToHtmlColor()}'><b>Общий долг водителя: {unclosedAdvanceMoney.ToShortCurrencyString()}</b></span>"
				: "";

			if(defectiveBottlesReturnedToWarehouse > 0) {
				lblQtyOfDefectiveGoods.Visible = true;
				lblQtyOfDefectiveGoods.Markup = string.Format(
					"Единиц брака: <b>{0}</b> шт.",
						defectiveBottlesReturnedToWarehouse);
				var namesOfDefectiveReasons = defectiveReasons.Select(x => x.GetEnumTitle());
				DefectSourceLabel.Text = " Причины: " + String.Join(", ", namesOfDefectiveReasons);
			} else {
				lblQtyOfDefectiveGoods.Visible = false;
				DefectSourceLabel.Visible = false;
			}

			var bottleDifference = bottlesReturnedToWarehouse - bottlesReturnedTotal;
			var differenceAttributes = bottlesReturnedToWarehouse - bottlesReturnedTotal > 0 ? "background=\"#ff5555\"" : "";
			var bottleDifferenceFormat = "<span {1}><b>{0}</b><sub>(осталось)</sub></span>";
			checkUseBottleFine.Visible = bottleDifference < 0;
			if(bottleDifference != 0) {
				checkUseBottleFine.Label = string.Format("({0:C})", DefaultBottle.SumOfDamage * (-bottleDifference));
			}
			labelBottleDifference.Markup = string.Format(bottleDifferenceFormat, bottleDifference, differenceAttributes);

			//Штрафы
			decimal totalSumOfDamage = 0;
			if(checkUseBottleFine.Active)
				totalSumOfDamage += DefaultBottle.SumOfDamage * (-bottleDifference);
			totalSumOfDamage += routelistdiscrepancyview.Items.Where(x => x.UseFine).Sum(x => x.SumOfDamage);

			StringBuilder fineText = new StringBuilder();
			if(totalSumOfDamage != 0) {
				fineText.AppendLine(string.Format("Выб. ущерб: {0:C}", totalSumOfDamage));
			}
			if(Entity.BottleFine != null) {
				fineText.AppendLine(string.Format("Штраф: {0:C}", Entity.BottleFine.TotalMoney));
			}
			labelBottleFine.LabelProp = fineText.ToString().TrimEnd('\n');
			buttonBottleAddEditFine.Sensitive = totalSumOfDamage != 0;
			buttonBottleDelFine.Sensitive = Entity.BottleFine != null;
			if(buttonFineEditState != (Entity.BottleFine != null)) {
				(buttonBottleAddEditFine.Image as Image).Pixbuf = new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(),
					Entity.BottleFine != null ? "Vodovoz.icons.buttons.edit.png" : "Vodovoz.icons.buttons.add.png"
				);
				buttonFineEditState = Entity.BottleFine != null;
			}

			UpdateYbuttonCashChangeReturnSensitivity();
		}

		protected bool IsConsistentWithUnloadDocument()
		{
			var hasItemsDiscrepancies = routelistdiscrepancyview.Items.Any(discrepancy => discrepancy.Remainder != 0);
			bool hasFine = Entity.BottleFine != null;
			var items = Entity.Addresses.Where(item => item.IsDelivered());
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned + item.Order.BottlesByStockActualCount);
			var hasTotalBottlesDiscrepancy = bottlesReturnedToWarehouse != bottlesReturnedTotal;
			return hasFine || (!hasTotalBottlesDiscrepancy && !hasItemsDiscrepancies) || Entity.DifferencesConfirmed;
		}

		public override bool Save() {

			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return false;
			}

			var contextItems = new Dictionary<object, object>
			{
				{nameof(IRouteListItemRepository), _routeListItemRepository},
				{nameof(DriverTerminalCondition), _needToSelectTerminalCondition && Entity.Status == RouteListStatus.Closed},
				{Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders, _canCreateRouteListWithoutOrders},
			};

			var context = new ValidationContext(Entity, null, contextItems);
			var validator = ServicesConfig.ValidationService;

			permissioncommentview.Save();

			if(!validator.Validate(Entity, context))
			{
				return false;
			}

			if(!ValidateOrders()) {
				return false;
			}

			if(!TrySetCashier()) {
				return false;
			}

			if(Entity.Status == RouteListStatus.Delivered)
			{
				Entity.ChangeStatusAndCreateTask(
					Entity.GetCarVersion.IsCompanyCar && Entity.Car.CarModel.CarTypeOfUse != CarTypeOfUse.Truck
						? RouteListStatus.MileageCheck
						: RouteListStatus.OnClosing,
					CallTaskWorker
				);
			}

			foreach(var address in Entity.Addresses)
			{
				_paymentFromBankClientController.UpdateAllocatedSum(UoW, address.Order);
				_paymentFromBankClientController.ReturnAllocatedSumToClientBalanceIfChangedPaymentTypeFromCashless(UoW, address.Order);
			}

			UoW.Save();

			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			UoW.Save(Entity.RouteListProfitability);
			UoW.Commit();
			
			return true;
		}

		private bool ValidateOrders()
		{
			bool isOrdersValid = true;
			string orderIds = "";
			byte ordersCounter = 0;
			ValidationContext validationContext;

			foreach(var item in Entity.Addresses) {
				validationContext = new ValidationContext(item.Order);
				validationContext.Items.Add(Order.ValidationKeyIgnoreReceipts, _ignoreReceiptsForOrderIds.Contains(item.Order.Id));
				validationContext.ServiceContainer.AddService(_orderSettings);
				validationContext.ServiceContainer.AddService(_deliveryRulesSettings);
				if(!ServicesConfig.ValidationService.Validate(item.Order, validationContext))
				{
					if(string.IsNullOrWhiteSpace(orderIds)) {
						orderIds = string.Format("{0}", item.Order.Id);
					} else {
						orderIds = string.Format("{0}{2} {1}", orderIds, item.Order.Id, ordersCounter == 4 ? "\n" : ",");
					}
					isOrdersValid = false;
					if(ordersCounter == 4) {
						ordersCounter = 0;
						continue;
					}
					ordersCounter++;
				}
			}

			if(!isOrdersValid) {
				MessageDialogHelper.RunErrorDialog(string.Format("Следующие заказы заполнены некорректно:\n {0}", orderIds));
				return false;
			}
			return true;
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			PerformanceHelper.StartMeasurement();
			
			if(!TrySetCashier()) {
				return;
			}

			var validationContext = _validationContextFactory.CreateNewValidationContext(
				Entity,
				new Dictionary<object, object> {
					{"NewStatus", RouteListStatus.MileageCheck},
					{"cash_order_close", true},
					{nameof(IRouteListItemRepository), _routeListItemRepository},
					{nameof(DriverTerminalCondition), _needToSelectTerminalCondition},
					{RouteList.ValidationKeyIgnoreReceiptsForOrders, _ignoreReceiptsForOrderIds},
					{Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders, _canCreateRouteListWithoutOrders},
				});
			validationContext.ServiceContainer.AddService(_orderSettings);
			validationContext.ServiceContainer.AddService(_deliveryRulesSettings);

			if(!ServicesConfig.ValidationService.Validate(Entity, validationContext))
			{
				return;
			}

			PerformanceHelper.AddTimePoint("Валидация МЛ");
			
			if(advanceCheckbox.Active && advanceSpinbutton.Value > 0) {
				EmployeeAdvanceOrder((decimal)advanceSpinbutton.Value);
				
				PerformanceHelper.AddTimePoint("Создан расходный ордер");
			}

			NewDriverAdvanceModel newDriverAdvanceModel = new NewDriverAdvanceModel(_newDriverAdvanceSettings, _routeListRepository, Entity);
			bool needNewDriverAdvance = _isOpenFromCash && newDriverAdvanceModel.NeedNewDriverAdvance(UoW);
			bool hasDriverUnclosedRouteLists = newDriverAdvanceModel.UnclosedRouteLists(UoW).Any();
			if(needNewDriverAdvance)
			{
				if(hasDriverUnclosedRouteLists
				   && !MessageDialogHelper.RunQuestionDialog(
					   "У водителя есть незакрытые МЛ:\n"
					   + newDriverAdvanceModel.UnclosedRouteListStrings(UoW) 
					   + "\nТекущий МЛ будет закрыт без выдачи аванса.\nПродолжить?"))
				{
					return;
				}

				if (!hasDriverUnclosedRouteLists)
				{
					var newDriverAdvanceSumParameter = _newDriverAdvanceSettings.NewDriverAdvanceSum;
					var driverWage = Entity.GetDriversTotalWage();
					if(driverWage > 0)
					{
						var newDriverAdvanceSum = driverWage > newDriverAdvanceSumParameter ? newDriverAdvanceSumParameter : driverWage * 0.5m;
						newDriverAdvanceModel.CreateNewDriverAdvance(UoW, _financialCategoriesGroupsSettings, newDriverAdvanceSum);
					}
				}
			}

			var cash = _cashRepository.CurrentRouteListCash(UoW, Entity.Id);
			if(Entity.Total != cash) {
				MessageDialogHelper.RunWarningDialog($"Невозможно подтвердить МЛ, сумма МЛ ({CurrencyWorks.GetShortCurrencyString(Entity.Total)}) не соответствует кассе ({CurrencyWorks.GetShortCurrencyString(cash)}).");
				if(Entity.Status == RouteListStatus.OnClosing && Entity.ConfirmedDistance <= 0 && Entity.NeedMileageCheck && MessageDialogHelper.RunQuestionDialog("По МЛ не принят километраж, перевести в статус проверки километража?")) {
					Entity.ChangeStatusAndCreateTask(RouteListStatus.MileageCheck, CallTaskWorker);
					
					PerformanceHelper.AddTimePoint("Статус сменен на 'проверка километража' и создано задание");
				}
				return;
			}

			if(Entity.GetCarVersion.CarOwnType == CarOwnType.Raskat)
			{
				Entity.RecountMileage();
			}

			Entity.UpdateMovementOperations(_financialCategoriesGroupsSettings);

			PerformanceHelper.AddTimePoint("Обновлены операции перемещения");

			if(Entity.Status == RouteListStatus.OnClosing) {
				Entity.AcceptCash(CallTaskWorker);
				
				PerformanceHelper.AddTimePoint("Создано задание на обзвон");
			}

			if(Entity.Status == RouteListStatus.Delivered) {
				if(routelistdiscrepancyview.Items.Any(discrepancy => discrepancy.Remainder != 0)
				&& !Entity.DifferencesConfirmed) {
					Entity.ChangeStatusAndCreateTask(RouteListStatus.OnClosing, CallTaskWorker);
				} else {
					if(Entity.GetCarVersion.IsCompanyCar && Entity.Car.CarModel.CarTypeOfUse != CarTypeOfUse.Truck) {
						Entity.ChangeStatusAndCreateTask(RouteListStatus.MileageCheck, CallTaskWorker);
					} else {
						Entity.ChangeStatusAndCreateTask(RouteListStatus.Closed, CallTaskWorker);
					}
				}
			}
			
			Entity.WasAcceptedByCashier = true;

			if(needNewDriverAdvance && !hasDriverUnclosedRouteLists)
			{
				ShowCashSummaryMessage();
			}

			SaveAndClose();
			
			PerformanceHelper.AddTimePoint("Сохранение и закрытие завершено");
			
			PerformanceHelper.Main.PrintAllPoints(_logger);
		}

		private void ShowCashSummaryMessage()
		{
			var income = _cashRepository.GetIncomeSumByRouteListId(UoW, Entity.Id);
			var expenseWithEmployeeAdvance = _cashRepository.GetExpenseSumByRouteListId(UoW, Entity.Id, new ExpenseType[] { ExpenseType.EmployeeAdvance });
			var expenseWithoutEmployeeAdvance =
				_cashRepository.GetExpenseSumByRouteListId(UoW, Entity.Id, null, new ExpenseType[] { ExpenseType.EmployeeAdvance });

			StringBuilder resultMessageBuilder = new StringBuilder();
			resultMessageBuilder.AppendLine($"<span size=\"x-large\">Приходные ордера на сумму { income.ToString("N2") } руб.</span>");
			resultMessageBuilder.AppendLine($"<span color=\"{GdkColors.DangerText.ToHtmlColor()}\" size=\"x-large\">Расходные ордера на сумму { expenseWithoutEmployeeAdvance.ToString("N2") } руб.</span>");
			resultMessageBuilder.AppendLine($"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\" size=\"x-large\">Аванс на сумму {expenseWithEmployeeAdvance.ToString("N2")} руб.</span>");

			MessageDialogHelper.RunInfoDialog(resultMessageBuilder.ToString());
		}

		void PrintSelectedDocument(RouteListPrintDocuments choise)
		{
			if(!MessageDialogHelper.RunQuestionDialog("Перед печатью необходимо сохранить документ.\nСохранить?"))
				return;
			UoW.Save();

			switch(choise) {
				case RouteListPrintDocuments.All:
					PrintRouteList();
					PrintFines();
					break;
				case RouteListPrintDocuments.RouteList:
					PrintRouteList();
					break;
				case RouteListPrintDocuments.Fines:
					PrintFines();
					break;
			}
		}

		void PrintRouteList()
		{
			{
				var document = Additions.Logistic.PrintRouteListHelper.GetRDLRouteList(UoW, Entity);
				var reportDlg = new QSReport.ReportViewDlg(document);
				reportDlg.ReportPrinted += SavePrintTime;
				this.TabParent.OpenTab(
					QS.Dialog.Gtk.TdiTabBase.GenerateHashName<QSReport.ReportViewDlg>(),
					() => reportDlg);
			}
		}

		void SavePrintTime(object sender, EventArgs e)
		{
			Entity.AddPrintHistory();
			UoW.Save();
		}

		void PrintFines()
		{
			{
				var document = Additions.Logistic.PrintRouteListHelper.GetRDLFine(Entity, UoW);
				this.TabParent.OpenTab(
					QS.Dialog.Gtk.TdiTabBase.GenerateHashName<QSReport.ReportViewDlg>(),
					() => new QSReport.ReportViewDlg(document));
			}
		}

		protected void OnButtonBottleAddEditFineClicked(object sender, EventArgs e)
		{
			string fineReason = "Недосдача";
			var bottleDifference = bottlesReturnedTotal - bottlesReturnedToWarehouse;
			var summ = DefaultBottle.SumOfDamage * (bottleDifference > 0 ? bottleDifference : 0m);
			summ += routelistdiscrepancyview.Items.Where(x => x.UseFine).Sum(x => x.SumOfDamage);

			var nomenclatures = routelistdiscrepancyview.Items.Where(x => x.UseFine)
				.ToDictionary(x => x.Nomenclature, x => -x.Remainder);

			if(checkUseBottleFine.Active)
			{
				nomenclatures.Add(DefaultBottle, bottleDifference);
			}

			if(Entity.BottleFine != null) {
				var page = NavigationManager.OpenViewModelOnTdi<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.BottleFine.Id), OpenPageOptions.AsSlave);

				Entity.BottleFine.UpdateNomenclature(nomenclatures);
				page.ViewModel.Entity.TotalMoney = summ;
				page.ViewModel.EntitySaved += FineDlgExist_EntitySaved;
			} else {
				var page = NavigationManager.OpenViewModelOnTdi<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

				page.ViewModel.Entity.Fill(summ, Entity, fineReason, DateTime.Now, Entity.Driver);
				page.ViewModel.Entity.AddNomenclature(nomenclatures);
				page.ViewModel.EntitySaved += FineDlgNew_EntitySaved;
			}
		}

		void FineDlgNew_EntitySaved(object sender, QS.Tdi.EntitySavedEventArgs e)
		{
			Entity.BottleFine = e.Entity as Fine;
			CalculateTotal();
			UpdateButtonState();
		}

		void FineDlgExist_EntitySaved(object sender, QS.Tdi.EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(Entity.BottleFine);
			CalculateTotal();
		}

		protected void OnButtonBottleDelFineClicked(object sender, EventArgs e)
		{
			var fineId = Entity.BottleFine.Id;
			UoW.Session.Evict(Entity.BottleFine);
			Entity.BottleFine = null;
			OrmMain.DeleteObject<Fine>(fineId, UoW);
			CalculateTotal();
			UpdateButtonState();
		}

		protected void OnCheckUseBottleFineToggled(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		private void GetFuelInfo()
		{
			track = _trackRepository.GetTrackByRouteListId(UoW, Entity.Id);

			var fuelOtlayedOp = UoWGeneric.Root.FuelOutlayedOperation;
			var givedOp = Entity.FuelDocuments.Select(x => x.FuelOperation.Id);
			//Проверяем существование операций и исключаем их.
			var exclude = new List<int>();
			exclude.AddRange(givedOp);
			if(fuelOtlayedOp != null && fuelOtlayedOp.Id != 0) {
				exclude.Add(fuelOtlayedOp.Id);
			}
			if(exclude.Count == 0)
				exclude = null;

			if(Entity.Car.FuelType != null) {
				Employee driver = Entity.Driver;
				var car = Entity.Car;

				if(car.GetActiveCarVersionOnDate(Entity.Date).IsCompanyCar)
				{
					driver = null;
				}
				else
				{
					car = null;
				}

				balanceBeforeOp = _fuelRepository.GetFuelBalance(
					UoW, driver, car, Entity.ClosingDate ?? DateTime.Now, exclude?.ToArray());
			}
		}

		private void UpdateFuelInfo()
		{
			var text = new List<string>();
			decimal spentFuel = (decimal)Entity.Car.FuelConsumption / 100 * Entity.ConfirmedDistance;

			bool hasTrack = track?.Distance.HasValue ?? false;

			if(Entity.PlanedDistance != null && Entity.PlanedDistance != 0) {
				text.Add(string.Format("Планируемое расстояние: {0:F1} км", Entity.PlanedDistance));
			}
			if(hasTrack) {
				text.Add(string.Format("Расстояние по треку: {0:F1} км.", track.TotalDistance));
			}
			text.Add(string.Format("Расстояние подтвержденное логистами: {0:F1} км.", Entity.ConfirmedDistance));

			if(Entity.Car.FuelType != null) {
				text.Add(string.Format("Вид топлива: {0}", Entity.Car.FuelType.Name));
			} else {
				text.Add("Не указан вид топлива");
			}

			if(Entity.FuelDocuments.Select(x => x.FuelOperation).Any()) {
				text.Add(string.Format("Остаток без выдачи {0:F2} л.", balanceBeforeOp));
			}

			text.Add(string.Format("Израсходовано топлива: {0:F2} л. ({1:F2} л/100км)", spentFuel, (decimal)Entity.Car.FuelConsumption));

			if(Entity.FuelDocuments.Select(x => x.FuelOperation).Any()) {
				text.Add(string.Format("Выдано {0:F2} литров",
					 Entity.FuelDocuments.Select(x => x.FuelOperation.LitersGived).Sum()));
			}

			if(Entity.Car.FuelType != null) {
				text.Add(
					string.Format(
						"Текущий остаток топлива {0:F2} л.",
						balanceBeforeOp + Entity.FuelDocuments.Select(x => x.FuelOperation.LitersGived).Sum() - spentFuel
					)
				);
			}

			text.Add($"Номер топливной карты: {Entity.Car.GetCurrentActiveFuelCardVersion()?.FuelCard?.CardNumber}");

			ytextviewFuelInfo.Buffer.Text = string.Join("\n", text);
		}

		void LoadDataFromFine()
		{
			if(Entity.BottleFine == null)
				return;

			foreach(var nom in Entity.BottleFine.Nomenclatures) {
				if(nom.Nomenclature.Id == DefaultBottle.Id) {
					checkUseBottleFine.Active = true;
					continue;
				}

				var found = routelistdiscrepancyview.Items.FirstOrDefault(x => x.Nomenclature.Id == nom.Nomenclature.Id);
				if(found != null)
					found.UseFine = true;
			}
		}

		protected void OnYspinActualDistanceValueChanged(object sender, EventArgs e)
		{
			UpdateFuelInfo();
		}

		void ObservableFuelDocuments_ElementAdded(object aList, int[] aIdx)
		{
			UpdateFuelInfo();
			CalculateTotal();
		}

		void ObservableFuelDocuments_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			UpdateFuelInfo();
			CalculateTotal();
		}

		protected void OnYcheckConfirmDifferencesToggled(object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		protected void OnYcheckHideCellsToggled(object sender, EventArgs e)
		{
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
		}

		protected void OnButtonReturnedRefreshClicked(object sender, EventArgs e)
		{
			ReloadDiscrepancies();
		}

		private void ReloadDiscrepancies()
		{
			ReloadReturnedToWarehouse();
			routelistdiscrepancyview.FindDiscrepancies();
			CalculateTotal();
		}

		private void ReloadReturnedToWarehouse()
		{
			allReturnsToWarehouse = _routeListRepository.GetReturnsToWarehouse(UoW, Entity.Id, Nomenclature.GetCategoriesForShipment());
			bottlesReturnedToWarehouse = (int)_routeListRepository.GetReturnsToWarehouse(
				UoW,
				Entity.Id,
				_nomenclatureSettings.ReturnedBottleNomenclatureId)
			.Sum(item => item.Amount);

			var defectiveNomenclaturesIds = _nomenclatureRepository
				.GetNomenclatureOfDefectiveGoods(UoW)
				.Select(n => n.Id)
				.ToArray();

			var returnedDefectiveItems = _routeListRepository.GetReturnsToWarehouse(
				UoW,
				Entity.Id,
				defectiveNomenclaturesIds);

			defectiveReasons = returnedDefectiveItems
				.Select(x => x.DefectSource).Distinct();
			
			defectiveBottlesReturnedToWarehouse = (int)returnedDefectiveItems.Sum(item => item.Amount);
		}

		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			OrmMain.GetObjectDescription<CarUnloadDocument>().ObjectUpdatedGeneric -= OnCalUnloadUpdated;
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			routeListAddressesView.BottlesReturnedEdited -= OnBottlesReturnedEdited;
			base.Destroy();
		}

		protected void OnButtonCreateCashOrderClicked(object sender, EventArgs e)
		{
			var messages = new List<string>();

			if(!TrySetCashier()) {
				return;
			}

			Income cashIncome = null;
			Expense cashExpense = null;

			var inputCashOrder = (decimal)spinCashOrder.Value;
			try
			{
				messages.AddRange(Entity.ManualCashOperations(ref cashIncome, ref cashExpense, inputCashOrder, _financialCategoriesGroupsSettings));
			}
			catch(MissingOrdersWithCashlessPaymentTypeException ex)
			{
				MessageDialogHelper.RunErrorDialog(ex.Message);
			}

			if (cashIncome != null) UoW.Save(cashIncome);
			if (cashExpense != null) UoW.Save(cashExpense);

			Entity.UpdateRouteListDebt();

			UoW.Save();

			CalculateTotal();

			if(messages.Any())
				MessageDialogHelper.RunInfoDialog(string.Format("Были выполнены следующие действия:\n*{0}", string.Join("\n*", messages)));
		}

		private void EmployeeAdvanceOrder(decimal cashInput)
		{
			string message, ifAdvanceIsBigger;

			Expense cashExpense = null;
			decimal cashToReturn = Entity.MoneyToReturn - cashInput;

			ifAdvanceIsBigger = (cashToReturn > 0) ? "Сумма для сдачи в кассу" : "Сумма для выдачи из кассы";

			if(!TrySetCashier()) {
				return;
			}

			message = Entity.EmployeeAdvanceOperation(ref cashExpense, cashInput, _financialCategoriesGroupsSettings);

			if(cashExpense != null)
				UoW.Save(cashExpense);
			cashExpense.UpdateWagesOperations(UoW);
			UoW.Save();

			MessageDialogHelper.RunInfoDialog(string.Format("{0}\n\n{1}: {2:C0}", message, ifAdvanceIsBigger, Math.Abs(cashToReturn)));
		}

		private bool TrySetCashier()
		{
			var cashier = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(cashier == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете закрыть МЛ, так как некого указывать в качестве кассира.");
				return false;
			}

			Entity.Cashier = cashier;
			return true;
		}

		bool CheckIfCashier()
		{
			var cashSubdivisions = _subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });
			return cashSubdivisions.Contains(_employeeRepository.GetEmployeeForCurrentUser(UoW)?.Subdivision);
		}

		protected void OnAdvanceCheckboxToggled(object sender, EventArgs e)
		{
			advanceSpinbutton.Visible = advanceCheckbox.Active;
		}

		protected void OnAdvanceSpinbuttonChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		protected void OnButtonDeleteFuelDocumentClicked(object sender, EventArgs e)
		{
			FuelDocument fd = ytreeviewFuelDocuments.GetSelectedObject<FuelDocument>();
			if(fd == null) {
				return;
			}

			if(fd.FuelLimitLitersAmount > 0 || fd.FuelLimit != null)
			{
				MessageDialogHelper.RunErrorDialog("Нельзя удалить талоны по которым выдавались топливные лимиты");
				return;
			}

			if(fd.FuelCashExpense != null)
			{
				var cashTransferDocumentsHavingExpense =
					_cashRepository.GetCashTransferDocumentsIdsByExpenseId(UoW, fd.FuelCashExpense.Id);

				if(cashTransferDocumentsHavingExpense.Count > 0)
				{
					MessageDialogHelper.RunErrorDialog(
						$"Удалить талоны невозможно, т.к. расходный ордер талона задействован " +
						$"в документах перемещении ДС: {string.Join(", ", cashTransferDocumentsHavingExpense)}");

					return;
				}
			}

			var cashDistributionDocumentsIds =
				_cashRepository.GetCashDistributionDocumentsIdsByFuelDocumentId(UoW, fd.Id);

			if(cashDistributionDocumentsIds.Count > 0)
			{
				if(!MessageDialogHelper.RunQuestionDialog("Вы действительно хотите удалить талон выдачи топлива вместе документом расхода налички?"))
				{
					return;
				}

				_cashRepository.DeleteFuelExpenseCashDistributionDocuments(UoW, cashDistributionDocumentsIds);
			}

			Entity.ObservableFuelDocuments.Remove(fd);
		}

		protected void OnButtonAddFuelDocumentClicked(object sender, EventArgs e)
		{
			TabParent.OpenTab(
				$"FuelDocument_{DialogHelper.GenerateDialogHashName<RouteList>(Entity.Id)}",
				() => new FuelDocumentViewModel(
				UoW,
				Entity,
				ServicesConfig.CommonServices,
				_subdivisionRepository,
				_lifetimeScope.Resolve<IEmployeeRepository>(),
				_fuelRepository,
				NavigationManager,
				_lifetimeScope.Resolve<ITrackRepository>(),
				_lifetimeScope.Resolve<IEmployeeJournalFactory>(),
				_financialCategoriesGroupsSettings,
				_lifetimeScope.Resolve<IOrganizationRepository>(),
				_lifetimeScope.Resolve<IFuelApiService>(),
				_lifetimeScope.Resolve<IFuelControlSettings>(),
				_lifetimeScope.Resolve<IGuiDispatcher>(),
				_lifetimeScope.Resolve<IUserSettingsService>(),
				_lifetimeScope.Resolve<IYesNoCancelQuestionInteractive>(),
				_lifetimeScope
				));
		}

		protected void OnYtreeviewFuelDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			TabParent.OpenTab(
				$"FuelDocument_{DialogHelper.GenerateDialogHashName<RouteList>(Entity.Id)}",
				() => new FuelDocumentViewModel(
				UoW,
				ytreeviewFuelDocuments.GetSelectedObject<FuelDocument>(),
				ServicesConfig.CommonServices,
				_subdivisionRepository,
				_lifetimeScope.Resolve<IEmployeeRepository>(),
				_fuelRepository,
				NavigationManager,
				_lifetimeScope.Resolve<ITrackRepository>(),
				_lifetimeScope.Resolve<IEmployeeJournalFactory>(),
				_financialCategoriesGroupsSettings,
				_lifetimeScope.Resolve<IOrganizationRepository>(),
				_lifetimeScope.Resolve<IFuelApiService>(),
				_lifetimeScope.Resolve<IFuelControlSettings>(),
				_lifetimeScope.Resolve<IGuiDispatcher>(),
				_lifetimeScope.Resolve<IUserSettingsService>(),
				_lifetimeScope.Resolve<IYesNoCancelQuestionInteractive>(),
				_lifetimeScope
				));
		}

		protected void OnButtonCalculateCashClicked(object sender, EventArgs e)
		{
			var messages = new List<string>();

			if(!TrySetCashier()) {
				return;
			}

			if(Entity.FuelOperationHaveDiscrepancy()) {
				if(!MessageDialogHelper.RunQuestionDialog("Был изменен водитель или автомобиль, баланс по топливу изменится с учетом этих изменений. Продолжить?")) {
					return;
				}
			}

			var operationsResultMessage = Entity.UpdateCashOperations(_financialCategoriesGroupsSettings);
			messages.AddRange(operationsResultMessage);

			CalculateTotal();

			if(messages.Any()) {
				MessageDialogHelper.RunInfoDialog(string.Format("Были выполнены следующие действия:\n*{0}", string.Join("\n*", messages)));
			} else {
				MessageDialogHelper.RunInfoDialog("Сумма по кассе соответствует сумме МЛ.");
			}
		}

		#endregion

		#region Toggle buttons

		protected void OnToggleClosingToggled(object sender, EventArgs e)
		{
			if(toggleClosing.Active)
			{
				notebook1.CurrentPage = 0;
			}
		}

		protected void OnToggleWageToggled(object sender, EventArgs e)
		{
			if(toggleWageDetails.Active)
			{
				notebook1.CurrentPage = 1;
				textWageDetails.Buffer.Text = Entity.GetWageCalculationDetails(_wageParameterService);
			}
		}

		#endregion
	}
}
