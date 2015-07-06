using System;
using Gtk;
using NHibernate;
using NLog;
using QSBanks;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz.Domain;

public partial class MainWindow: Gtk.Window
{
	private static Gtk.Clipboard clipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));
	private static Logger logger = LogManager.GetCurrentClassLogger ();
	uint uiIdOrders, uiIdServices;

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

		UsersAction.Sensitive = QSMain.User.admin;
		labelUser.LabelProp = QSMain.User.Name;

		BanksUpdater.Update (false);
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
		if ((args.Event.Key == Gdk.Key.Cyrillic_es || args.Event.Key == Gdk.Key.Cyrillic_ES) && args.Event.State == modifier) {
			Widget w = (o as MainWindow).Focus;
			CopyToClipboard (w);
		}//CTRL+X
		else if ((args.Event.Key == Gdk.Key.Cyrillic_che || args.Event.Key == Gdk.Key.Cyrillic_CHE) && args.Event.State == modifier) {
			Widget w = (o as MainWindow).Focus;
			CutToClipboard (w);
		}//CTRL+V
		else if ((args.Event.Key == Gdk.Key.Cyrillic_em || args.Event.Key == Gdk.Key.Cyrillic_EM) && args.Event.State == modifier) {
			Widget w = (o as MainWindow).Focus;
			PasteFromClipboard (w);
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
		if (ActionOrders.Active) {
			uiIdOrders = this.UIManager.AddUiFromResource ("Vodovoz.toolbars.orders.xml");
			this.UIManager.EnsureUpdate ();
		} else if (uiIdOrders > 0) {
			this.UIManager.RemoveUi (uiIdOrders);
		}
	}

	protected void OnActionNewOrderActivated (object sender, EventArgs e)
	{
		throw new NotImplementedException ();
	}

	protected void OnActionServicesToggled (object sender, EventArgs e)
	{
		if (ActionServices.Active) {
			uiIdServices = this.UIManager.AddUiFromResource ("Vodovoz.toolbars.services.xml");
			this.UIManager.EnsureUpdate ();
		} else if (uiIdServices > 0) {
			this.UIManager.RemoveUi (uiIdServices);
		}
	}

	protected void OnActionLogisticsToggled (object sender, EventArgs e)
	{
		if (ActionLogistics.Active) {
			uiIdServices = this.UIManager.AddUiFromResource ("logistics.xml");
			this.UIManager.EnsureUpdate ();
		} else if (uiIdServices > 0) {
			this.UIManager.RemoveUi (uiIdServices);
		}
	}

	protected void OnActionStockToggled (object sender, EventArgs e)
	{
		if (ActionStock.Active) {
			uiIdServices = this.UIManager.AddUiFromResource ("warehouse.xml");
			this.UIManager.EnsureUpdate ();
		} else if (uiIdServices > 0) {
			this.UIManager.RemoveUi (uiIdServices);
		}
	}

	protected void OnActionOrganizationsActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var orgs = session.CreateCriteria<Organization> ();

		OrmReference refWin = new OrmReference (typeof(Organization), session, orgs);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionBanksRFActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Bank> ();

		OrmReference refWin = new OrmReference (typeof(Bank), session, criteria, "{QSBanks.Bank} Bik[БИК]; Name[Название]; City[Город];");
		tdiMain.AddTab (refWin);
	}

	protected void OnActionNationalityActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Nationality> ();

		OrmReference refWin = new OrmReference (typeof(Nationality), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEmploeyActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Employee> ();

		OrmReference refWin = new OrmReference (typeof(Employee), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCarsActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Car> ();

		OrmReference refWin = new OrmReference (typeof(Car), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionUnitsActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<MeasurementUnits> ();

		OrmReference refWin = new OrmReference (typeof(MeasurementUnits), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionColorsActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<EquipmentColors> ();

		OrmReference refWin = new OrmReference (typeof(EquipmentColors), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionManufacturersActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Manufacturer> ();

		OrmReference refWin = new OrmReference (typeof(Manufacturer), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEquipmentTypesActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<EquipmentType> ();

		OrmReference refWin = new OrmReference (typeof(EquipmentType), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionNomenclatureActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Nomenclature> ();

		OrmReference refWin = new OrmReference (typeof(Nomenclature), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionPhoneTypesActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<PhoneType> ();

		OrmReference refWin = new OrmReference (typeof(PhoneType), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCounterpartyHandbookActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Counterparty> ();

		OrmReference refWin = new OrmReference (typeof(Counterparty), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionSignificanceActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Significance> ();

		OrmReference refWin = new OrmReference (typeof(Significance), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionStatusActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<CounterpartyStatus> ();

		OrmReference refWin = new OrmReference (typeof(CounterpartyStatus), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEMailTypesActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<EmailType> ();

		OrmReference refWin = new OrmReference (typeof(EmailType), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCounterpartyPostActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Post> ();

		OrmReference refWin = new OrmReference (typeof(Post), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionFreeRentPackageActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<FreeRentPackage> ();

		OrmReference refWin = new OrmReference (typeof(FreeRentPackage), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionPaidRentPackageActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<PaidRentPackage> ();

		OrmReference refWin = new OrmReference (typeof(PaidRentPackage), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionEquipmentActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Equipment> ();

		OrmReference refWin = new OrmReference (typeof(Equipment), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionDeliveryScheduleActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<DeliverySchedule> ();

		OrmReference refWin = new OrmReference (typeof(DeliverySchedule), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionLogisticsAreaActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<LogisticsArea> ();

		OrmReference refWin = new OrmReference (typeof(LogisticsArea), session, criteria);
		tdiMain.AddTab (refWin);	
	}

	protected void OnActionUpdateBanksActivated (object sender, EventArgs e)
	{
		BanksUpdater.Update (true);
	}

	protected void OnActionWarehousesActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Warehouse> ();

		OrmReference refWin = new OrmReference (typeof(Warehouse), session, criteria);
		tdiMain.AddTab (refWin);	
	}

	protected void OnActionProductSpecificationActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<ProductSpecification> ();

		OrmReference refWin = new OrmReference (typeof(ProductSpecification), session, criteria);
		tdiMain.AddTab (refWin);
	}

	protected void OnActionCullingCategoryActivated (object sender, EventArgs e)
	{
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<CullingCategory> ();

		OrmReference refWin = new OrmReference (typeof(CullingCategory), session, criteria);
		tdiMain.AddTab (refWin);
	}
}
