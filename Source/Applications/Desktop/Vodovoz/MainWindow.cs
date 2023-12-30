using Autofac;
using NLog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Versioning;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Utilities.Debug;
using QS.Validation;
using QSBanks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Gtk;
using QS.Project.Services;
using QSProjectsLib;
using Vodovoz;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.MainMenu;
using Vodovoz.Parameters;
using Vodovoz.SidePanel;
using Vodovoz.TempAdapters;
using VodovozInfrastructure.Configuration;

public partial class MainWindow : Gtk.Window
{
	private static Logger _logger = LogManager.GetCurrentClassLogger();
	private uint _lastUiId;
	private readonly ILifetimeScope _autofacScope = Startup.AppDIContainer.BeginLifetimeScope();
	private readonly IPasswordValidator _passwordValidator;
	private readonly IApplicationConfigurator _applicationConfigurator;
	private IMovementDocumentsNotificationsController _movementsNotificationsController;
	private IComplaintNotificationController _complaintNotificationController;
	private bool _hasAccessToSalariesForLogistics;
	private int _currentUserSubdivisionId;
	private IEnumerable<int> _curentUserMovementDocumentsNotificationWarehouses;
	private bool _hideComplaintsNotifications;

	private bool _accessOnlyToWarehouseAndComplaints;

	public MainWindow(
		IPasswordValidator passwordValidator,
		IApplicationInfo applicationInfo,
		IApplicationConfigurator applicationConfigurator) : base(Gtk.WindowType.Toplevel)
	{
		_passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
		ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
		_applicationConfigurator = applicationConfigurator ?? throw new ArgumentNullException(nameof(applicationConfigurator));

		Build();

		TdiMain = tdiMain;
		InfoPanel = infopanel;
		ToolbarMain = toolbarMain;
		ToolbarComplaints = tlbComplaints;

		PerformanceHelper.AddTimePoint("Закончена стандартная сборка окна.");
	}
	
	public void Configure()
	{
		BuildToolbarActions();
		ConfigureMainMenu();

		tdiMain.WidgetResolver = ViewModelWidgetResolver.Instance;
		TDIMain.MainNotebook = tdiMain;
		_hideComplaintsNotifications = CurrentUserSettings.Settings.HideComplaintNotification;
		var tabsParametersProvider = new TabsParametersProvider(new ParametersProvider());
		TDIMain.SetTabsColorHighlighting(
			CurrentUserSettings.Settings.HighlightTabsWithColor,
			CurrentUserSettings.Settings.KeepTabColor,
			GetTabsColors(),
			tabsParametersProvider.TabsPrefix);
		TDIMain.SetTabsReordering(CurrentUserSettings.Settings.ReorderTabs);

		bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		if (isWindows)
		{
			KeyPressEvent += HotKeyHandler.HandleKeyPressEvent;
		}

		Title = $"{ApplicationInfo.ProductTitle} v{ApplicationInfo.Version.Major}.{ApplicationInfo.Version.Minor} от {GetDateTimeFGromVersion(ApplicationInfo.Version):dd.MM.yyyy HH:mm}";

		//Настраиваем модули

		var admin = QSMain.User.Admin;
		
		labelUser.LabelProp = QSMain.User.Name;
		var commonServices = ServicesConfig.CommonServices;
		var cashier = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.RoleCashier);
		ActionCash.Sensitive = cashier;
		ActionAccounting.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("money_manage_bookkeeping");
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
		var currentWarehousePermissions = new CurrentWarehousePermissions();
		ActionStock.Sensitive = currentWarehousePermissions.WarehousePermissions.Any(x => x.PermissionValue == true);

		var hasAccessToCRM = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_crm");
		ActionCRM.Sensitive = hasAccessToCRM;

		ActionFinesJournal.Visible = ActionPremiumJournal.Visible =
			commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
		ActionReports.Sensitive = false;
		//ActionServices.Visible = false;
		//ActionService.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance");
		ActionEmployeeWorkChart.Sensitive = false;

		ActionAddOrder.Sensitive = commonServices.PermissionService.ValidateUserPermission(typeof(Order), QSMain.User.Id)?.CanCreate ?? false;
		ActionExportImportNomenclatureCatalog.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
		ActionDistricts.Sensitive = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet)).CanRead;
		ActionDriversStopLists.Sensitive = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverStopListRemoval)).CanRead;

		// Отдел продаж

		ActionSalesDepartment.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_sales_department");

		#region Пользователь с правом работы только со складом и рекламациями

		using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			_accessOnlyToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;
		}

		MainMenuBar.Visible = ActionOrders.Visible = ActionServices.Visible = ActionLogistics.Visible = ActionCash.Visible =
			ActionAccounting.Visible = ActionReports.Visible = ActionArchive.Visible = ActionStaff.Visible = ActionCRM.Visible =
				ActionSuppliers.Visible = ActionCashRequest.Visible = ActionRetail.Visible = ActionCarService.Visible =
					MangoAction.Visible = !_accessOnlyToWarehouseAndComplaints;

		#endregion

		#region Уведомление об отправленных перемещениях для подразделения

		using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			_currentUserSubdivisionId = GetEmployeeSubdivisionId(uow);
			_curentUserMovementDocumentsNotificationWarehouses = CurrentUserSettings.Settings.MovementDocumentsNotificationUserSelectedWarehouses;
			_movementsNotificationsController = _autofacScope
				.Resolve<IMovementDocumentsNotificationsController>(
					new TypedParameter(typeof(int), _currentUserSubdivisionId),
					new TypedParameter(typeof(IEnumerable<int>), _curentUserMovementDocumentsNotificationWarehouses));

			var notificationDetails = _movementsNotificationsController.GetNotificationDetails(uow);
			UpdateSentMovementsNotification(notificationDetails.Notification);
			hboxMovementsNotification.Visible = notificationDetails.NeedNotify;

			if(notificationDetails.NeedNotify)
			{
				_movementsNotificationsController.UpdateNotificationAction += UpdateSentMovementsNotification;
			}
		}

		btnUpdateNotifications.Clicked += OnBtnUpdateNotificationClicked;

		#endregion

		#region Уведомление о наличии незакрытых рекламаций без комментариев в добавленной дискуссии для отдела

		_complaintNotificationController = _autofacScope.Resolve<IComplaintNotificationController>(new TypedParameter(typeof(int), _currentUserSubdivisionId));

		if (!_hideComplaintsNotifications)
		{
			_complaintNotificationController.UpdateNotificationAction += UpdateSendedComplaintsNotification;

			var complaintNotificationDetails = GetComplaintNotificationDetails();
			UpdateSendedComplaintsNotification(complaintNotificationDetails);

			btnOpenComplaint.Clicked += OnBtnOpenComplaintClicked;
		}
		else
		{
			hboxComplaintsNotification.Visible = false;
		}
		#endregion

		hboxNotifications.Visible = hboxMovementsNotification.Visible || !_hideComplaintsNotifications;

		BanksUpdater.CheckBanksUpdate(false);

		// Настройки розницы

		var userHaveAccessToRetail = commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");

		ActionRetail.Sensitive = userHaveAccessToRetail;
		ActionRetailUndeliveredOrdersJournal.Sensitive = false; // Этот журнал не готов - выключено до реализации фичи
	}

	private void ConfigureMainMenu()
	{
		var menuCreator = _autofacScope.Resolve<MainMenuBarCreator>();
		MainMenuBar = menuCreator.CreateMenuBar();
		vboxMain.Add(MainMenuBar);

		var box = (Box.BoxChild)vboxMain[MainMenuBar];
		box.Position = 0;
		box.Expand = false;
	}

	public IApplicationInfo ApplicationInfo { get; }
	public TdiNotebook TdiMain { get; }
	public InfoPanel InfoPanel { get; }
	public Toolbar ToolbarMain { get; }
	public Toolbar ToolbarComplaints { get; }
	public MenuBar MainMenuBar { get; private set; }
	
	public ITdiCompatibilityNavigation NavigationManager { get; private set; }
	public MangoManager MangoManager { get; private set; }
	
	private string[] GetTabsColors() =>
		new[] { "#F81919", "#009F6B", "#1F8BFF", "#FF9F00", "#FA7A7A", "#B46034", "#99B6FF", "#8F2BE1", "#00CC44" };

	/// <summary>
	/// Пока в <see cref="EmployeeJournalFactory"/> есть получение <see cref="NavigationManager"/> через <see cref="MainWindow"/>
	/// то инициализируем менеджеры после инициализации главного окна
	/// </summary>
	public void InitializeManagers()
	{
		NavigationManager = _autofacScope.Resolve<ITdiCompatibilityNavigation>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		MangoManager = _autofacScope.Resolve<MangoManager>(new TypedParameter(typeof(Gtk.Action), MangoAction));
		MangoManager.Connect();
	}

	private DateTime GetDateTimeFGromVersion(Version version) =>
		new DateTime(2000, 1, 1)
			.AddDays(version.Build)
			.AddSeconds(version.Revision * 2);
}
