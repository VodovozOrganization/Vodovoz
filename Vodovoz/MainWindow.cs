using System;
using System.Linq;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Project.Dialogs;
using QS.Tdi.Gtk;
using QSBanks;
using QSBusinessCommon.Domain;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz;
using Vodovoz.Core;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Dialogs.OnlineStore;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Store;
using Vodovoz.JournalViewers;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.Representations;
using Vodovoz.ServiceDialogs;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModel;

public partial class MainWindow : Gtk.Window, IProgressBarDisplayable
{
	private static Logger logger = LogManager.GetCurrentClassLogger();
	uint LastUiId;

	public TdiNotebook TdiMain {
		get {
			return tdiMain;
		}
	}

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();
		PerformanceHelper.AddTimePoint("Закончена стандартная сборка окна.");
		this.BuildToolbarActions();
		this.KeyReleaseEvent += ClipboardWorkaround.HandleKeyReleaseEvent;
		TDIMain.MainNotebook = tdiMain;
		this.KeyReleaseEvent += TDIMain.TDIHandleKeyReleaseEvent;
		//Передаем лебл
		QSMain.StatusBarLabel = labelStatus;
		this.Title = MainSupport.GetTitle();
		QSMain.MakeNewStatusTargetForNlog();

		//Настраиваем модули
		MainClass.SetupAppFromBase();

		UsersAction.Sensitive = QSMain.User.Admin;
		ActionParameters.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		ActionCash.Sensitive = QSMain.User.Permissions["money_manage_cash"];
		ActionAccounting.Sensitive = QSMain.User.Permissions["money_manage_bookkeeping"];
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive = QSMain.User.Permissions["logistican"];
		ActionStock.Sensitive = CurrentPermissions.Warehouse.Allowed().Any();

		bool hasAccessToSalaries = QSMain.User.Permissions["access_to_salaries"];
		bool hasAccessToWagesAndBonuses = QSMain.User.Permissions["access_to_fines_bonuses"];
		ActionEmployeesBonuses.Sensitive = hasAccessToWagesAndBonuses;
		ActionEmployeeFines.Sensitive = hasAccessToWagesAndBonuses;
		ActionEmployeesBonuses.Sensitive = hasAccessToWagesAndBonuses;
		ActionDriverWages.Sensitive = hasAccessToSalaries;
		ActionWagesOperations.Sensitive = hasAccessToSalaries;
		ActionForwarderWageReport.Sensitive = hasAccessToSalaries;

		ActionFinesJournal.Visible = ActionPremiumJournal.Visible = QSMain.User.Permissions["access_to_fines_bonuses"];
		ActionReports.Sensitive = false;
		ActionServices.Visible = false;
		ActionMaintenance.Sensitive = QSMain.User.Permissions["database_maintenance"];

		unreadedMessagesWidget.MainTab = tdiMain;
		//Читаем настройки пользователя
		switch(CurrentUserSettings.Settings.ToolbarStyle) {
			case ToolbarStyle.Both:
				ActionToolBarBoth.Activate();
				break;
			case ToolbarStyle.Icons:
				ActionToolBarIcon.Activate();
				break;
			case ToolbarStyle.Text:
				ActionToolBarText.Activate();
				break;
		}

		switch(CurrentUserSettings.Settings.ToolBarIconsSize) {
			case IconsSize.ExtraSmall:
				ActionIconsExtraSmall.Activate();
				break;
			case IconsSize.Small:
				ActionIconsSmall.Activate();
				break;
			case IconsSize.Middle:
				ActionIconsMiddle.Activate();
				break;
			case IconsSize.Large:
				ActionIconsLarge.Activate();
				break;
		}

		BanksUpdater.CheckBanksUpdate(false);
	}

	#region IProgressBarDisplayable implementation

	public void ProgressStart(double maxValue, double minValue = 0, string text = null, double startValue = 0)
	{
		progressStatus.Adjustment = new Adjustment(startValue, minValue, maxValue, 1, 1, 1);
		progressStatus.Text = text;
		progressStatus.Visible = true;
		QSMain.WaitRedraw();
	}

	public void ProgressUpdate(double curValue)
	{
		if(progressStatus == null || progressStatus.Adjustment == null)
			return;
		progressStatus.Adjustment.Value = curValue;
		QSMain.WaitRedraw();
	}

	public void ProgressUpdate(string curText)
	{
		if(progressStatus == null || progressStatus.Adjustment == null)
			return;
		progressStatus.Text = curText;
		QSMain.WaitRedraw();
	}

	public void ProgressAdd(double addValue = 1, string text = null)
	{
		if(progressStatus == null)
			return;
		progressStatus.Adjustment.Value += addValue;
		if(text != null)
			progressStatus.Text = text;
		if(progressStatus.Adjustment.Value > progressStatus.Adjustment.Upper)
			logger.Warn("Значение ({0}) прогресс бара в статусной строке больше максимального ({1})",
						(int)progressStatus.Adjustment.Value,
						(int)progressStatus.Adjustment.Upper
					   );
		QSMain.WaitRedraw();
	}

	public void ProgressClose()
	{
		progressStatus.Text = null;
		progressStatus.Visible = false;
		QSMain.WaitRedraw();
	}

	#endregion

	public void OnTdiMainTabAdded(object sender, TabAddedEventArgs args)
	{
		var currentTab = args.Tab;
		if(currentTab is IInfoProvider)
			(currentTab as IInfoProvider).CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
	}

	public void OnTdiMainTabClosed(object sender, TabClosedEventArgs args)
	{
		var closedTab = args.Tab;
		if(closedTab is IInfoProvider)
			infopanel.OnInfoProviderDisposed(closedTab as IInfoProvider);
		if(tdiMain.NPages == 0)
			infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
	}

	public void OnTdiMainTabSwitched(object sender, TabSwitchedEventArgs args)
	{
		var currentTab = args.Tab;
		if(currentTab is IInfoProvider)
			infopanel.SetInfoProvider(currentTab as IInfoProvider);
		else
			infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		if(tdiMain.CloseAllTabs()) {
			a.RetVal = false;
			MainClass.TrayIcon.Dispose();
			Application.Quit();
		} else {
			a.RetVal = true;
		}
	}

	protected void OnQuitActionActivated(object sender, EventArgs e)
	{
		if(tdiMain.CloseAllTabs()) {
			Application.Quit();
		}
	}

	protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
	{
		QSMain.User.ChangeUserPassword(this);
	}

	protected void OnAction3Activated(object sender, EventArgs e)
	{
		Users winUsers = new Users();
		winUsers.Show();
		winUsers.Run();
		winUsers.Destroy();
	}

	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		QSMain.RunAboutDialog();
	}

	protected void OnActionOrdersToggled(object sender, EventArgs e)
	{
		if(ActionOrders.Active)
			SwitchToUI("Vodovoz.toolbars.orders.xml");
	}

	private void SwitchToUI(string uiResource)
	{
		if(LastUiId > 0) {
			this.UIManager.RemoveUi(LastUiId);
			LastUiId = 0;
		}
		LastUiId = this.UIManager.AddUiFromResource(uiResource);
		this.UIManager.EnsureUpdate();
	}

	protected void OnActionServicesToggled(object sender, EventArgs e)
	{
		if(ActionServices.Active)
			SwitchToUI("Vodovoz.toolbars.services.xml");
	}

	protected void OnActionLogisticsToggled(object sender, EventArgs e)
	{
		if(ActionLogistics.Active)
			SwitchToUI("logistics.xml");
	}

	protected void OnActionStockToggled(object sender, EventArgs e)
	{
		if(ActionStock.Active)
			SwitchToUI("warehouse.xml");
	}

	protected void OnActionOrganizationsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Organization));
		tdiMain.AddTab(refWin);
	}

	protected void OnSubdivisionsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Subdivision>(),
			() => new OrmReference(typeof(Subdivision))
		);
	}

	protected void OnActionBanksRFActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Bank));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionNationalityActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Nationality));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCitizenshipActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Citizenship));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionEmploeyActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<EmployeesVM>(),
			() => new ReferenceRepresentation(new EmployeesVM())
		);
	}

	protected void OnActionCarsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Car));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionUnitsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(MeasurementUnits));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionDiscountReasonsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(DiscountReason));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionColorsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(EquipmentColors));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionManufacturersActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Manufacturer));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionEquipmentTypesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(EquipmentType));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionNomenclatureActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Nomenclature));
		refWin.ButtonMode = ReferenceButtonMode.CanEdit;
		refWin.ButtonMode |= QSMain.User.Permissions["can_create_and_arc_nomenclatures"] ? ReferenceButtonMode.CanAdd : ReferenceButtonMode.None;
		refWin.ButtonMode |= QSMain.User.Permissions["can_delete_nomenclatures"] ? ReferenceButtonMode.CanDelete : ReferenceButtonMode.None;
		tdiMain.AddTab(refWin);
	}

	protected void OnActionPhoneTypesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(PhoneType));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCounterpartyHandbookActivated(object sender, EventArgs e)
	{
		var refWin = new ReferenceRepresentation(new CounterpartyVM());
		refWin.ButtonMode = QSMain.User.Permissions["can_delete_counterparty_and_deliverypoint"] ? ReferenceButtonMode.CanAll : (ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanEdit);
		tdiMain.AddTab(refWin);
	}

	protected void OnActionEMailTypesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(EmailType));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCounterpartyPostActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Post));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionFreeRentPackageActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(FreeRentPackage));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionPaidRentPackageActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(PaidRentPackage));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionEquipmentActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Equipment));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionDeliveryScheduleActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(DeliverySchedule));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionLogisticsAreaActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<LogisticAreasEditDlg>(),
			() => new LogisticAreasEditDlg()
		);
	}

	protected void OnActionUpdateBanksFromCBRActivated(object sender, EventArgs e)
	{
		BanksUpdater.CheckBanksUpdate(true);
	}

	protected void OnActionWarehousesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<WarehousesView>(),
			() => new WarehousesView()
		);
	}

	protected void OnActionProductSpecificationActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(ProductSpecification));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCullingCategoryActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(CullingCategory));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCommentTemplatesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(CommentTemplate));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionLoad1cActivated(object sender, EventArgs e)
	{
		var win = new LoadFrom1cDlg();
		tdiMain.AddTab(win);
	}

	protected void OnActionRouteColumnsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(RouteColumn));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionFuelTypeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<FuelType>(),
			() => new OrmReference(typeof(FuelType))
		);
	}

	protected void OnActionDeliveryShiftActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(DeliveryShift));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionParametersActivated(object sender, EventArgs e)
	{
		var config = new ApplicationConfigDialog();
		config.ShowAll();
		config.Run();
		config.Destroy();
	}

	protected void OnAction14Activated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(IncomeCategory));
		tdiMain.AddTab(refWin);
	}

	protected void OnAction15Activated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(ExpenseCategory));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCashToggled(object sender, EventArgs e)
	{
		if(ActionCash.Active)
			SwitchToUI("cash.xml");
	}

	protected void OnActionAccountingToggled(object sender, EventArgs e)
	{
		if(ActionAccounting.Active)
			SwitchToUI("accounting.xml");
	}

	protected void OnActionDocTemplatesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DocTemplate>(),
			() => new OrmReference(typeof(DocTemplate))
		);
	}

	protected void OnActionToolBarTextToggled(object sender, EventArgs e)
	{
		if(ActionToolBarText.Active)
			ToolBarMode(ToolbarStyle.Text);
	}

	private void ToolBarMode(ToolbarStyle style)
	{
		if(CurrentUserSettings.Settings.ToolbarStyle != style) {
			CurrentUserSettings.Settings.ToolbarStyle = style;
			CurrentUserSettings.SaveSettings();
		}
		toolbarMain.ToolbarStyle = style;
		ActionIconsExtraSmall.Sensitive = ActionIconsSmall.Sensitive = ActionIconsMiddle.Sensitive = ActionIconsLarge.Sensitive =
			style != ToolbarStyle.Text;
	}

	private void ToolBarMode(IconsSize size)
	{
		if(CurrentUserSettings.Settings.ToolBarIconsSize != size) {
			CurrentUserSettings.Settings.ToolBarIconsSize = size;
			CurrentUserSettings.SaveSettings();
		}
		switch(size) {
			case IconsSize.ExtraSmall:
				toolbarMain.IconSize = IconSize.SmallToolbar;
				break;
			case IconsSize.Small:
				toolbarMain.IconSize = IconSize.LargeToolbar;
				break;
			case IconsSize.Middle:
				toolbarMain.IconSize = IconSize.Dnd;
				break;
			case IconsSize.Large:
				toolbarMain.IconSize = IconSize.Dialog;
				break;
		}
	}

	protected void OnActionToolBarIconToggled(object sender, EventArgs e)
	{
		if(ActionToolBarIcon.Active)
			ToolBarMode(ToolbarStyle.Icons);
	}

	protected void OnActionToolBarBothToggled(object sender, EventArgs e)
	{
		if(ActionToolBarBoth.Active)
			ToolBarMode(ToolbarStyle.Both);
	}

	protected void OnActionIconsExtraSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsExtraSmall.Active)
			ToolBarMode(IconsSize.ExtraSmall);
	}

	protected void OnActionIconsSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsSmall.Active)
			ToolBarMode(IconsSize.Small);
	}

	protected void OnActionIconsMiddleToggled(object sender, EventArgs e)
	{
		if(ActionIconsMiddle.Active)
			ToolBarMode(IconsSize.Middle);
	}

	protected void OnActionIconsLargeToggled(object sender, EventArgs e)
	{
		if(ActionIconsLarge.Active)
			ToolBarMode(IconsSize.Large);
	}

	protected void OnActionDeliveryPointsActivated(object sender, EventArgs e)
	{
		ReferenceButtonMode mode = ReferenceButtonMode.CanEdit;
		if(QSMain.User.Permissions["can_delete_counterparty_and_deliverypoint"])
			mode |= ReferenceButtonMode.CanDelete;

		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<DeliveryPointsVM>(),
			() => new ReferenceRepresentation(new DeliveryPointsVM()).Buttons(mode)
		);
	}

	protected void OnPropertiesActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			DialogHelper.GenerateDialogHashName<UserSettings>(CurrentUserSettings.Settings.Id),
			() => new UserSettingsDlg(CurrentUserSettings.Settings)
		);
	}

	protected void OnActionTransportationWagonActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<MovementWagon>(),
			() => new OrmReference(typeof(MovementWagon))
		);
	}

	protected void OnActionRegrandingOfGoodsTempalteActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<RegradingOfGoodsTemplate>(),
			() => new OrmReference(typeof(RegradingOfGoodsTemplate))
		);
	}

	protected void OnActionEmployeeFinesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EmployeesFines>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EmployeesFines())
		);
	}

	protected void OnActionStockMovementsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.StockMovements>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.StockMovements())
		);
	}

	protected void OnActionArchiveToggled(object sender, EventArgs e)
	{
		if(ActionArchive.Active)
			SwitchToUI("archive.xml");
	}

	protected void OnActionStaffToggled(object sender, EventArgs e)
	{
		if(ActionStaff.Active)
			SwitchToUI("Vodovoz.toolbars.staff.xml");
	}

	protected void OnActionSalesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.SalesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.SalesReport())
		);
	}
	protected void OnActionDriverWagesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.DriverWagesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.DriverWagesReport())
		);
	}
	protected void OnActionFuelReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.FuelReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.FuelReport())
		);
	}
	protected void OnActionShortfallBattlesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Bottles.ShortfallBattlesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Bottles.ShortfallBattlesReport())
		);
	}
	protected void OnActionWagesOperationsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.WagesOperationsReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.WagesOperationsReport())
		);
	}

	protected void OnActionEquipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EquipmentReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EquipmentReport())
		);
	}

	protected void OnActionForwarderWageReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.ForwarderWageReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.ForwarderWageReport())
		);
	}

	protected void OnActionCashierCommentsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.CashierCommentsReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.CashierCommentsReport())
		);
	}

	protected void OnActionCommentsForLogistsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OnecCommentsReport>(),
			() => new QSReport.ReportViewDlg(new OnecCommentsReport())
		);
	}

	protected void OnActionDriversWageBalanceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.DriversWageBalanceReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.DriversWageBalanceReport())
		);
	}

	protected void OnActionFineCommentTemplatesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(FineTemplate));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionDeliveriesLateActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.DeliveriesLateReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.DeliveriesLateReport())
		);
	}

	protected void OnActionRoutesListRegisterActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.RoutesListRegisterReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.RoutesListRegisterReport())
		);
	}

	protected void OnActionDeliveryTimeReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.DeliveryTimeReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.DeliveryTimeReport())
		);
	}

	protected void OnActionOrdersByDistrict(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByDistrictReport())
		);
	}

	protected void OnActionCompanyTrucksActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CompanyTrucksReport>(),
			() => new QSReport.ReportViewDlg(new CompanyTrucksReport())
		);
	}

	protected void OnActionLastOrderReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<LastOrderByDeliveryPointReport>(),
			() => new QSReport.ReportViewDlg(new LastOrderByDeliveryPointReport())
		);
	}


	protected void OnActionOrderIncorrectPricesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderIncorrectPrices>(),
			() => new QSReport.ReportViewDlg(new OrderIncorrectPrices())
		);
	}

	protected void OnActionAddressDuplicetesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<MergeAddressesDlg>(),
			() => new MergeAddressesDlg()
		);
	}

	protected void OnActionOrdersWithMinPriceLessThanActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersWithMinPriceLessThan>(),
			() => new QSReport.ReportViewDlg(new OrdersWithMinPriceLessThan())
		);
	}

	protected void OnActionRouteListsOnClosingActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport())
		);
	}

	protected void OnActionOnLoadTimeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.OnLoadTimeAtDayReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.OnLoadTimeAtDayReport())
		);
	}

	protected void OnActionSelfDeliveryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SelfDeliveryReport>(),
			() => new QSReport.ReportViewDlg(new SelfDeliveryReport())
		);
	}

	protected void OnActionDeliveryDayScheduleActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DeliveryDaySchedule>(),
			() => new OrmReference(typeof(DeliveryDaySchedule))
		);
	}

	protected void OnActionShipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.ShipmentReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.ShipmentReport())
		);
	}

	protected void OnActionBottlesMovementReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Bottles.BottlesMovementReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Bottles.BottlesMovementReport())
		);
	}

	protected void OnActionMileageReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.MileageReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.MileageReport())
		);
	}

	protected void OnActionMastersReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MastersReport>(),
			() => new QSReport.ReportViewDlg(new MastersReport())
		);
	}

	protected void OnActionSuburbWaterPriceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Sales.SuburbWaterPriceReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Sales.SuburbWaterPriceReport())
		);
	}

	protected void OnActionDistanceFromCenterActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<CalculateDistanceToPointsDlg>(),
			() => new CalculateDistanceToPointsDlg()
		);
	}

	protected void OnActionOrdersWithoutBottlesOperationActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<OrdersWithoutBottlesOperationDlg>(),
			() => new OrdersWithoutBottlesOperationDlg()
		);
	}

	protected void OnActionHistoryLogActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<QS.HistoryLog.Dialogs.HistoryView>(),
			() => new QS.HistoryLog.Dialogs.HistoryView()
		);
	}

	protected void OnIncomeBalanceReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<IncomeBalanceReport>(),
			() => new QSReport.ReportViewDlg(new IncomeBalanceReport())
		);
	}

	protected void OnAction45Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ReplaceEntityLinksDlg>(),
			() => new ReplaceEntityLinksDlg()
		);
	}

	protected void OnActionBottlesMovementSummaryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Bottles.BottlesMovementSummaryReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Bottles.BottlesMovementSummaryReport())
		);
	}

	protected void OnActionDriveingCallsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.DrivingCallReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.DrivingCallReport())
		);
	}

	protected void OnActionMastersVisitReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MastersVisitReport>(),
			() => new QSReport.ReportViewDlg(new MastersVisitReport())
		);
	}

	protected void OnActionNotDeliveredOrdersActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NotDeliveredOrdersReport>(),
			() => new QSReport.ReportViewDlg(new NotDeliveredOrdersReport())
		);
	}

	protected void OnActionCounterpartyTagsActivated(object sender, EventArgs e)
	{
		var refWin = new OrmReference(typeof(Tag));
		tdiMain.AddTab(refWin);
	}

	protected void OnAction47Activated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(PremiumTemplate));
		tdiMain.AddTab(refWin);
	}

	protected void OnAction48Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesPremiums>(),
			() => new QSReport.ReportViewDlg(new EmployeesPremiums())
		);
	}

	protected void OnAction49Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderStatisticByWeekReport>(),
			() => new QSReport.ReportViewDlg(new OrderStatisticByWeekReport())
		);
	}

	protected void OnReportKungolovoActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReportForBigClient>(),
			() => new QSReport.ReportViewDlg(new ReportForBigClient())
		);
	}

	protected void OnActionLoad1cCounterpartyAndDeliveryPointsActivated(object sender, EventArgs e)
	{
		var widget = new LoadFrom1cClientsAndDeliveryPoints();
		tdiMain.AddTab(widget);
	}

	protected void OnActionFolders1cActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Folder1c>(),
			() => new OrmReference(typeof(Folder1c))
		);
	}

	protected void OnActionOrderRegistryActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderRegistryReport>(),
			() => new QSReport.ReportViewDlg(new OrderRegistryReport())
		);
	}

	protected void OnActionEquipmentBalanceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Store.EquipmentBalance>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Store.EquipmentBalance())
		);
	}

	protected void OnActionCardPaymentsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CardPaymentsOrdersReport>(),
			() => new QSReport.ReportViewDlg(new CardPaymentsOrdersReport())
		);
	}

	protected void OnActionCameFromActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<ClientCameFrom>(),
			() => new OrmReference(typeof(ClientCameFrom))
		);
	}

	protected void OnActionProductGroupsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<ProductGroup>(),
			() => new OrmReference(typeof(ProductGroup))
		);
	}

	protected void OnActionToOnlineStoreActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ExportToSiteDlg>(),
			() => new ExportToSiteDlg()
		);
	}

	protected void OnActionSendedBillsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SendedEmailsReport>(),
			() => new QSReport.ReportViewDlg(new SendedEmailsReport())
		);
	}

	protected void OnActionDefectiveItemsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DefectiveItemsReport>(),
			() => new QSReport.ReportViewDlg(new DefectiveItemsReport())
		);
	}

	protected void OnActionTraineeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<TraineeVM>(),
			() => new ReferenceRepresentation(new TraineeVM())
		);
	}

	protected void OnActionDeliveryPriceRulesActivated(object sender, EventArgs e)
	{
		bool right = QSMain.User.Permissions["can_edit_delivery_price_rules"];
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DeliveryPriceRule>(),
			() => {
				var dlg = new OrmReference(typeof(DeliveryPriceRule)) {
					ButtonMode = right ? ReferenceButtonMode.CanAll : ReferenceButtonMode.None
				};
				return dlg;
			}
		);
	}

	protected void OnOnLineActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromTinkoffReport>(),
			() => new QSReport.ReportViewDlg(new PaymentsFromTinkoffReport())
		);
	}

	protected void OnActionOrdersByDistrictsAndDeliverySchedulesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictsAndDeliverySchedulesReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByDistrictsAndDeliverySchedulesReport())
		);
	}

	protected void OnActionOrdersByCreationDate(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByCreationDateReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByCreationDateReport())
			);
	}
}
