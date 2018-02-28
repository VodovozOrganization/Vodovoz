using System;
using System.Linq;
using Gtk;
using NLog;
using QSBanks;
using QSBusinessCommon.Domain;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;
using QSTDI;
using Vodovoz;
using Vodovoz.Core;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.Panel;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModel;

public partial class MainWindow : Gtk.Window
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

		MainSupport.LoadBaseParameters();

		MainSupport.TestVersion(this); //Проверяем версию базы
		QSMain.CheckServer(this); // Проверяем настройки сервера

		PerformanceHelper.AddTimePoint("Закончена загрузка параметров базы и проверка версии.");

		if(QSMain.User.Login == "root") {
			string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
			MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
								   MessageType.Info,
								   ButtonsType.Ok,
								   Message);
			md.Run();
			md.Destroy();
			Users WinUser = new Users();
			WinUser.Show();
			WinUser.Run();
			WinUser.Destroy();
			return;
		}

		//Настраиваем модули
		MainClass.SetupAppFromBase();

		UsersAction.Sensitive = QSMain.User.Admin;
		ActionParameters.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		ActionCash.Sensitive = QSMain.User.Permissions["money_manage"];
		ActionAccounting.Sensitive = QSMain.User.Permissions["money_manage"];
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive = QSMain.User.Permissions["logistican"];
		ActionStock.Sensitive = CurrentPermissions.Warehouse.Allowed().Any();

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

		BanksUpdater.Update(false);
	}

	#region Прогресс в статус строке

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

	protected void OnActionEmploeyActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Employee));
		tdiMain.AddTab(refWin);
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
		tdiMain.AddTab(refWin);
	}

	protected void OnActionSignificanceActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Significance));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionStatusActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(CounterpartyStatus));
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
		OrmReference refWin = new OrmReference(typeof(LogisticsArea));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionUpdateBanksActivated(object sender, EventArgs e)
	{
		BanksUpdater.Update(true);
	}

	protected void OnActionWarehousesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Warehouse));
		tdiMain.AddTab(refWin);
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
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<DeliveryPointsVM>(),
			() => new ReferenceRepresentation(new DeliveryPointsVM()).Buttons(ReferenceButtonMode.CanEdit | ReferenceButtonMode.CanDelete)
		);
	}

	protected void OnPropertiesActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmMain.GenerateDialogHashName<UserSettings>(CurrentUserSettings.Settings.Id),
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
		var widget = new Vodovoz.Reports.EmployeesFines();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionStockMovementsActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.StockMovements();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
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
		var widget = new Vodovoz.Reports.SalesReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}
	protected void OnActionDriverWagesActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.DriverWagesReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}
	protected void OnActionFuelReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.FuelReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}
	protected void OnActionShortfallBattlesActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.ShortfallBattlesReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}
	protected void OnActionWagesOperationsActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.WagesOperationsReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionEquipmentReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.EquipmentReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionForwarderWageReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.ForwarderWageReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionCashierCommentsActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.CashierCommentsReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnAction1cCommentsActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.OnecCommentsReport();
		tdiMain.OpenTab(
					QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionDriversWageBalanceActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.DriversWageBalanceReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionFineCommentTemplatesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(FineTemplate));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionDeliveriesLateActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.Logistic.DeliveriesLateReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionRoutesListRegisterActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.Logistic.RoutesListRegisterReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionDeliveryTimeReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.Logistic.DeliveryTimeReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionOrdersByDistrict(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.OrdersByDistrictReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionCompanyTrucksActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.CompanyTrucksReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionLastOrderReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.LastOrderByDeliveryPointReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}


	protected void OnActionOrderIncorrectPricesReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.OrderIncorrectPrices();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
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
		var widget = new Vodovoz.ReportsParameters.OrdersWithMinPriceLessThan();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionRouteListsOnClosingActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionOnLoadTimeActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.Logistic.OnLoadTimeAtDayReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionSelfDeliveryReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.SelfDeliveryReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
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
		var widget = new Vodovoz.ReportsParameters.Logistic.ShipmentReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionBottlesMovementReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.Logistic.BottlesMovementReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionMileageReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.Logistic.MileageReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionMastersReportActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.MastersReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionSuburbWaterPriceActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.Sales.SuburbWaterPriceReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
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

	protected void OnAction43Activated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.IncomeBalanceReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnAction45Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ReplaceEntityLinksDlg>(),
			() => new ReplaceEntityLinksDlg()
		);
	}
}
