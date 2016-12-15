using System;
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
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.Panel;
using Vodovoz.ViewModel;

public partial class MainWindow: Gtk.Window
{
	private static Gtk.Clipboard clipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));
	private static Logger logger = LogManager.GetCurrentClassLogger ();
	uint LastUiId;

	public TdiNotebook TdiMain{
		get{
			return tdiMain;
		}
	}

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
		this.BuildToolbarActions ();
		this.KeyReleaseEvent += HandleKeyReleaseEvent;
		//Передаем лебл
		QSMain.StatusBarLabel = labelStatus;
		this.Title = MainSupport.GetTitle ();
		QSMain.MakeNewStatusTargetForNlog ();

		MainSupport.LoadBaseParameters ();

		MainSupport.TestVersion (this); //Проверяем версию базы
		QSMain.CheckServer (this); // Проверяем настройки сервера

		if (QSMain.User.Login == "root") {
			string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
			MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent,
				                   MessageType.Info, 
				                   ButtonsType.Ok,
				                   Message);
			md.Run ();
			md.Destroy ();
			Users WinUser = new Users ();
			WinUser.Show ();
			WinUser.Run ();
			WinUser.Destroy ();
			return;
		}

		UsersAction.Sensitive = QSMain.User.Admin;
		ActionParameters.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		ActionCash.Sensitive = QSMain.User.Permissions ["money_manage"];
		ActionAccounting.Sensitive = QSMain.User.Permissions ["money_manage"];
		ActionLogistics.Sensitive = QSMain.User.Permissions ["logistican"];

		unreadedMessagesWidget.MainTab = tdiMain;
		//Читаем настройки пользователя
		switch(CurrentUserSettings.Settings.ToolbarStyle)
		{
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

		switch(CurrentUserSettings.Settings.ToolBarIconsSize)
		{
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

		BanksUpdater.Update (false);
	}

	public void OnTdiMainTabAdded(object sender, TabAddedEventArgs args)
	{
		var currentTab = args.Tab;
		if (currentTab is IInfoProvider)
			(currentTab as IInfoProvider).CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
	}

	public void OnTdiMainTabClosed(object sender, TabClosedEventArgs args)
	{
		var closedTab = args.Tab;
		if(closedTab is IInfoProvider)
			infopanel.OnInfoProviderDisposed(closedTab as IInfoProvider);
		if (tdiMain.NPages == 0)
			infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
	}

	public void OnTdiMainTabSwitched(object sender, TabSwitchedEventArgs args)
	{
		var currentTab = args.Tab;
		if (currentTab is IInfoProvider)
			infopanel.SetInfoProvider(currentTab as IInfoProvider);
		else			
			infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
	}

	void HandleKeyReleaseEvent (object o, KeyReleaseEventArgs args)
	{
		int platform = (int)Environment.OSVersion.Platform;
		int version = (int)Environment.OSVersion.Version.Major;
		Gdk.ModifierType modifier;

		//Kind of MacOSX
		if ((platform == 4 || platform == 6 || platform == 128) && version > 8)
			modifier = Gdk.ModifierType.MetaMask | Gdk.ModifierType.Mod1Mask;
		//Kind of Windows or Unix
		else
			modifier = Gdk.ModifierType.ControlMask;

		//CTRL+C	
		if ((args.Event.Key == Gdk.Key.Cyrillic_es || args.Event.Key == Gdk.Key.Cyrillic_ES) && args.Event.State.HasFlag(modifier)) {
			Widget w = (o as MainWindow).Focus;
			CopyToClipboard (w);
		}//CTRL+X
		else if ((args.Event.Key == Gdk.Key.Cyrillic_che || args.Event.Key == Gdk.Key.Cyrillic_CHE) && args.Event.State.HasFlag(modifier)) {
			Widget w = (o as MainWindow).Focus;
			CutToClipboard (w);
		}//CTRL+V
		else if ((args.Event.Key == Gdk.Key.Cyrillic_em || args.Event.Key == Gdk.Key.Cyrillic_EM) && args.Event.State.HasFlag(modifier)) {
			Widget w = (o as MainWindow).Focus;
			PasteFromClipboard (w);
		}//CTRL+S || CTRL+ENTER
		else if ((args.Event.Key == Gdk.Key.S
		         || args.Event.Key == Gdk.Key.s
		         || args.Event.Key == Gdk.Key.Cyrillic_yeru
		         || args.Event.Key == Gdk.Key.Cyrillic_YERU
				|| args.Event.Key == Gdk.Key.Return) && args.Event.State.HasFlag(modifier)) {
			var w = tdiMain.CurrentPageWidget;
			if (w is QSTDI.TabVBox) {
				var tab = (w as QSTDI.TabVBox).Tab;
				if (tab is QSTDI.TdiSliderTab) {
					var dialog = (tab as QSTDI.TdiSliderTab).ActiveDialog;
					dialog.SaveAndClose ();
				}
				if(tab is ITdiDialog)
				{
					(tab as ITdiDialog).SaveAndClose();
				}
			}
		}
	}

	void CopyToClipboard (Widget w)
	{
		int start, end;

		if (w is Editable)
			(w as Editable).CopyClipboard ();
		else if (w is TextView)
			(w as TextView).Buffer.CopyClipboard (clipboard);
		else if (w is Label) {
			(w as Label).GetSelectionBounds (out start, out end);
			if (start != end)
				clipboard.Text = (w as Label).Text.Substring (start, end - start);
		}
	}

	void CutToClipboard (Widget w)
	{
		int start, end;

		if (w is Editable)
			(w as Editable).CutClipboard ();
		else if (w is TextView)
			(w as TextView).Buffer.CutClipboard (clipboard, true);
		else if (w is Label) {
			(w as Label).GetSelectionBounds (out start, out end);
			if (start != end)
				clipboard.Text = (w as Label).Text.Substring (start, end - start);
		}
	}

	void PasteFromClipboard (Widget w)
	{
		if (w is Editable)
			(w as Editable).PasteClipboard ();
		else if (w is TextView)
			(w as TextView).Buffer.PasteClipboard (clipboard);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		if (tdiMain.CloseAllTabs ()) {
			a.RetVal = false;
			MainClass.TrayIcon.Dispose();
			Application.Quit ();
		} else {
			a.RetVal = true;
		}
	}

	protected void OnQuitActionActivated (object sender, EventArgs e)
	{
		if (tdiMain.CloseAllTabs ()) {
			Application.Quit ();
		}
	}

	protected void OnDialogAuthenticationActionActivated (object sender, EventArgs e)
	{
		QSMain.User.ChangeUserPassword (this);
	}

	protected void OnAction3Activated (object sender, EventArgs e)
	{
		Users winUsers = new Users ();
		winUsers.Show ();
		winUsers.Run ();
		winUsers.Destroy ();
	}

	protected void OnAboutActionActivated (object sender, EventArgs e)
	{
		QSMain.RunAboutDialog ();
	}

	protected void OnActionOrdersToggled (object sender, EventArgs e)
	{
		if (ActionOrders.Active)
			SwitchToUI ("Vodovoz.toolbars.orders.xml");
	}

	private void SwitchToUI (string uiResource)
	{
		if (LastUiId > 0) {
			this.UIManager.RemoveUi (LastUiId);
			LastUiId = 0;
		}
		LastUiId = this.UIManager.AddUiFromResource (uiResource);
		this.UIManager.EnsureUpdate ();
	}

	protected void OnActionServicesToggled (object sender, EventArgs e)
	{
		if (ActionServices.Active)
			SwitchToUI ("Vodovoz.toolbars.services.xml");
	}

	protected void OnActionLogisticsToggled (object sender, EventArgs e)
	{
		if (ActionLogistics.Active)
			SwitchToUI ("logistics.xml");
	}

	protected void OnActionStockToggled (object sender, EventArgs e)
	{
		if (ActionStock.Active)
			SwitchToUI ("warehouse.xml");
	}

	protected void OnActionOrganizationsActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Organization));
		tdiMain.AddTab (refWin);
	}

	protected void OnSubdivisionsActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Subdivision));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionBanksRFActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Bank));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionNationalityActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Nationality));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEmploeyActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Employee));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCarsActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Car));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionUnitsActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(MeasurementUnits));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionColorsActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(EquipmentColors));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionManufacturersActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Manufacturer));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEquipmentTypesActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(EquipmentType));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionNomenclatureActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Nomenclature));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionPhoneTypesActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(PhoneType));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCounterpartyHandbookActivated (object sender, EventArgs e)
	{
		var refWin = new ReferenceRepresentation (new CounterpartyVM ());
		tdiMain.AddTab (refWin);
	}

	protected void OnActionSignificanceActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Significance));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionStatusActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(CounterpartyStatus));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEMailTypesActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(EmailType));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCounterpartyPostActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Post));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionFreeRentPackageActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(FreeRentPackage));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionPaidRentPackageActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(PaidRentPackage));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEquipmentActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Equipment));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionDeliveryScheduleActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(DeliverySchedule));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionLogisticsAreaActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(LogisticsArea));
		tdiMain.AddTab (refWin);	
	}

	protected void OnActionUpdateBanksActivated (object sender, EventArgs e)
	{
		BanksUpdater.Update (true);
	}

	protected void OnActionWarehousesActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(Warehouse));
		tdiMain.AddTab (refWin);	
	}

	protected void OnActionProductSpecificationActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(ProductSpecification));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCullingCategoryActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(CullingCategory));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCommentTemplatesActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(CommentTemplate));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionLoad1cActivated (object sender, EventArgs e)
	{
		var win = new LoadFrom1cDlg ();
		tdiMain.AddTab (win);
	}

	protected void OnActionRouteColumnsActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(RouteColumn));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionFuelTypeActivated (object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<FuelType>(),
			() => new OrmReference(typeof(FuelType))
		);
	}

	protected void OnActionDeliveryShiftActivated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(DeliveryShift));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionParametersActivated (object sender, EventArgs e)
	{
		var config = new ApplicationConfigDialog ();
		config.ShowAll ();
		config.Run ();
		config.Destroy ();
	}

	protected void OnAction14Activated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(IncomeCategory));
		tdiMain.AddTab (refWin);
	}

	protected void OnAction15Activated (object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference (typeof(ExpenseCategory));
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCashToggled (object sender, EventArgs e)
	{
		if (ActionCash.Active)
			SwitchToUI ("cash.xml");
	}

	protected void OnActionAccountingToggled (object sender, EventArgs e)
	{
		if (ActionAccounting.Active)
			SwitchToUI ("accounting.xml");
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
		if (ActionToolBarText.Active)
			ToolBarMode(ToolbarStyle.Text);
	}

	private void ToolBarMode(ToolbarStyle style)
	{
		if(CurrentUserSettings.Settings.ToolbarStyle != style)
		{
			CurrentUserSettings.Settings.ToolbarStyle = style;
			CurrentUserSettings.SaveSettings();
		}
		toolbarMain.ToolbarStyle = style;
		ActionIconsExtraSmall.Sensitive = ActionIconsSmall.Sensitive = ActionIconsMiddle.Sensitive = ActionIconsLarge.Sensitive =
			style != ToolbarStyle.Text;
	}

	private void ToolBarMode(IconsSize size)
	{
		if(CurrentUserSettings.Settings.ToolBarIconsSize != size)
		{
			CurrentUserSettings.Settings.ToolBarIconsSize = size;
			CurrentUserSettings.SaveSettings();
		}
		switch (size)
		{
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
		if (ActionToolBarIcon.Active)
			ToolBarMode(ToolbarStyle.Icons);
	}

	protected void OnActionToolBarBothToggled(object sender, EventArgs e)
	{
		if (ActionToolBarBoth.Active)
			ToolBarMode(ToolbarStyle.Both);
	}

	protected void OnActionIconsExtraSmallToggled(object sender, EventArgs e)
	{
		if (ActionIconsExtraSmall.Active)
			ToolBarMode(IconsSize.ExtraSmall);
	}
		
	protected void OnActionIconsSmallToggled(object sender, EventArgs e)
	{
		if (ActionIconsSmall.Active)
			ToolBarMode(IconsSize.Small);
	}

	protected void OnActionIconsMiddleToggled(object sender, EventArgs e)
	{
		if (ActionIconsMiddle.Active)
			ToolBarMode(IconsSize.Middle);
	}

	protected void OnActionIconsLargeToggled(object sender, EventArgs e)
	{
		if (ActionIconsLarge.Active)
			ToolBarMode(IconsSize.Large);
	}

	protected void OnActionDeliveryPointsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<DeliveryPointsVM>(),
			() => new ReferenceRepresentation (new DeliveryPointsVM ()).Buttons(ReferenceButtonMode.CanEdit | ReferenceButtonMode.CanDelete)
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
			() => new QSReport.ReportViewDlg (widget)
		);
	}

	protected void OnActionStockMovementsActivated(object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.StockMovements();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg (widget)
		);
	}

	protected void OnActionArchiveToggled(object sender, EventArgs e)
	{
		if (ActionArchive.Active)
			SwitchToUI ("archive.xml");
	}

	protected void OnGazTicketActionActivated (object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<GazTicket>(),
			() => new OrmReference(typeof(GazTicket))
		);
	}

	protected void OnActionSalesReportActivated (object sender, EventArgs e)
	{
		var widget = new Vodovoz.Reports.SalesReport();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg (widget)
		);
	}
}
