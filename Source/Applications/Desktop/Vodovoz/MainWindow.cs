using Autofac;
using MassTransit;
using NLog;
using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Versioning;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Utilities.Debug;
using QSBanks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Gtk;
using QS.Project.Services;
using QS.Services;
using QSProjectsLib;
using Vodovoz;
using Vodovoz.Application.Pacs;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.MainMenu;
using Vodovoz.Presentation.ViewModels.Pacs;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Tabs;
using Vodovoz.SidePanel;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Mango;
using Order = Vodovoz.Domain.Orders.Order;

public partial class MainWindow : Gtk.Window
{
	private static Logger _logger = LogManager.GetCurrentClassLogger();
	private uint _lastUiId;
	private readonly ILifetimeScope _autofacScope = Startup.AppDIContainer.BeginLifetimeScope();
	private readonly IInteractiveService _interactiveService;
	private readonly IWikiSettings _wikiSettings;
	private readonly OperatorService _operatorService;
	private IComplaintNotificationController _complaintNotificationController;
	private IMovementDocumentsNotificationsController _movementsNotificationsController;
	private IEnumerable<int> _curentUserMovementDocumentsNotificationWarehouses;
	private bool _accessOnlyToWarehouseAndComplaints;
	private bool _hideComplaintsNotifications;
	private bool _hasAccessToSalariesForLogistics;
	private int _currentUserSubdivisionId;

	public MainWindow(
		IInteractiveService interactiveService,
		IApplicationInfo applicationInfo,
		ICurrentPermissionService currentPermissionService,
		IWikiSettings wikiSettings,
		OperatorService operatorService) : base(Gtk.WindowType.Toplevel)
	{
		ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
		_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		_wikiSettings = wikiSettings ?? throw new ArgumentNullException(nameof(wikiSettings));
		_operatorService = operatorService ?? throw new ArgumentNullException(nameof(operatorService));
		CurrentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
		
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
		var tabsSettings = _autofacScope.Resolve<ITabsSettings>();
		TDIMain.SetTabsColorHighlighting(
			CurrentUserSettings.Settings.HighlightTabsWithColor,
			CurrentUserSettings.Settings.KeepTabColor,
			GetTabsColors(),
			tabsSettings.TabsPrefix);
		TDIMain.SetTabsReordering(CurrentUserSettings.Settings.ReorderTabs);

		bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		if(isWindows)
		{
			KeyPressEvent += HotKeyHandler.HandleKeyPressEvent;
		}

		Title = $"{ApplicationInfo.ProductTitle} v{ApplicationInfo.Version.Major}.{ApplicationInfo.Version.Minor} от {GetDateTimeFGromVersion(ApplicationInfo.Version):dd.MM.yyyy HH:mm}";

		//Настраиваем модули
		var admin = QSMain.User.Admin;

		pacspanelview1.ViewModel = _autofacScope.Resolve<PacsPanelViewModel>();
		
		labelUser.LabelProp = QSMain.User.Name;
		var commonServices = ServicesConfig.CommonServices;
		var cashier = CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier);
		ActionCash.Sensitive = cashier;
		ActionAccounting.Sensitive = CurrentPermissionService.ValidatePresetPermission("money_manage_bookkeeping");
		//Action1SWork.Sensitive = CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.BookkeeppingPermissions.Work1S.HasAccessTo1sWork);
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive =
				CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.IsLogistician);
		var currentWarehousePermissions = new CurrentWarehousePermissions();
		ActionStock.Sensitive = currentWarehousePermissions.WarehousePermissions.Any(x => x.PermissionValue == true);

		var hasAccessToCRM = CurrentPermissionService.ValidatePresetPermission("access_to_crm");
		ActionCRM.Sensitive = hasAccessToCRM;

		ActionFinesJournal.Visible = ActionPremiumJournal.Visible = CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
		ActionReports.Sensitive = false;
		//ActionServices.Visible = false;
		//ActionService.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance");
		ActionEmployeeWorkChart.Sensitive = false;

		ActionAddOrder.Sensitive = commonServices.PermissionService.ValidateUserPermission(typeof(Order), QSMain.User.Id)?.CanCreate ?? false;
		ActionExportImportNomenclatureCatalog.Sensitive =
			CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
		ActionDistricts.Sensitive = CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet)).CanRead;
		ActionDriversStopLists.Sensitive = CurrentPermissionService.ValidateEntityPermission(typeof(DriverStopListRemoval)).CanRead;

		// Отдел продаж

		ActionSalesDepartment.Sensitive = CurrentPermissionService.ValidatePresetPermission("access_to_sales_department");

		#region Пользователь с правом работы только со складом и рекламациями

		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			_accessOnlyToWarehouseAndComplaints =
				CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;
		}

		MainMenuBar.Visible = ActionOrders.Visible = ActionServices.Visible = ActionLogistics.Visible = ActionCash.Visible =
			ActionAccounting.Visible = ActionReports.Visible = ActionArchive.Visible = ActionStaff.Visible = ActionCRM.Visible =
				ActionSuppliers.Visible = ActionCashRequest.Visible = ActionRetail.Visible = ActionCarService.Visible =
					/*MangoAction.Visible =*/ !_accessOnlyToWarehouseAndComplaints;

		#endregion

		#region Уведомление об отправленных перемещениях для подразделения

		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			var employeeService = _autofacScope.Resolve<IEmployeeService>();

			_currentUserSubdivisionId = employeeService.GetEmployeeForCurrentUser()?.Subdivision?.Id ?? 0;
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

		if(!_hideComplaintsNotifications)
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
		var userHaveAccessToRetail = CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");

		ActionRetail.Sensitive = userHaveAccessToRetail;
		ActionRetailUndeliveredOrdersJournal.Sensitive = false; // Этот журнал не готов - выключено до реализации фичи
	}

	private void ConfigureMainMenu()
	{
		var menuCreator = _autofacScope.Resolve<MainMenuBarCreator>();
		MainMenuBar = menuCreator.CreateMenuBar();
		vboxMain.Add(MainMenuBar);

		//InitializeThemesMenuItem();

		OpenWikiCommand = new DelegateCommand(OpenWiki);
		ybuttonWiki.BindCommand(OpenWikiCommand);

		this.KeyPressEvent += OnKeyPressed;
		var box = (Box.BoxChild)vboxMain[MainMenuBar];
		box.Position = 0;
		box.Expand = false;
	}

	public ICurrentPermissionService CurrentPermissionService { get; }
	public IApplicationInfo ApplicationInfo { get; }
	public TdiNotebook TdiMain { get; }
	public InfoPanel InfoPanel { get; }
	public Toolbar ToolbarMain { get; }
	public Toolbar ToolbarComplaints { get; }
	public MenuBar MainMenuBar { get; private set; }
	

	private void OnKeyPressed(object o, Gtk.KeyPressEventArgs args)
	{
		if(args.Event.Key == Gdk.Key.F1)
		{
			OpenWikiCommand.Execute();
		}
	}

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
		MangoManager = _autofacScope.Resolve<MangoManager>();
	}

	private DateTime GetDateTimeFGromVersion(Version version) =>
		new DateTime(2000, 1, 1)
			.AddDays(version.Build)
			.AddSeconds(version.Revision * 2);

	public DelegateCommand OpenWikiCommand { get; private set; }

	private void OpenWiki()
	{
		Process.Start(new ProcessStartInfo(_wikiSettings.Url) { UseShellExecute = true });
	}

	protected override void OnDestroyed()
	{
		/*if(_messageBusControl != null)
        {
            _messageBusControl.Start();
        }*/

		base.OnDestroyed();
	}

	protected void OnServiceDeliveryRulesActivated(object sender, EventArgs e)
	{
	}
}
