using System;
using Gtk;
using QSProjectsLib;
using Vodovoz;
using QSSupportLib;
using NLog;

public partial class MainWindow: Gtk.Window
{
	private static Logger logger = LogManager.GetCurrentClassLogger();
	uint uiIdOrders, uiIdServices;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		//Передаем лебл
		MainClass.StatusBarLabel = labelStatus;
		QSMain.MakeNewStatusTargetForNlog("StatusMessage", "Vodovoz.MainClass, Vodovoz");
		Reference.RunReferenceItemDlg += OnRunReferenceItemDialog;
		QSMain.ReferenceUpdated += OnReferenceUpdate;

		//Test version of base
		try
		{
			MainSupport.BaseParameters = new BaseParam(QSMain.ConnectionDB);
		}
		catch(Exception e)
		{
			logger.FatalException("Не удалось получить информацию о версии базы данных.", e);
			MessageDialog BaseError = new MessageDialog ( this, DialogFlags.DestroyWithParent,
				MessageType.Warning, 
				ButtonsType.Close, 
				"Не удалось получить информацию о версии базы данных.");
			BaseError.Run();
			BaseError.Destroy();
			Environment.Exit(0);
		}

		MainSupport.ProjectVerion = new AppVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString(),
			"gpl",
			System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
		MainSupport.TestVersion(this); //Проверяем версию базы
		QSMain.CheckServer (this); // Проверяем настройки сервера

		if(QSMain.User.Login == "root")
		{
			string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
			MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
				MessageType.Info, 
				ButtonsType.Ok,
				Message);
			md.Run ();
			md.Destroy();
			Users WinUser = new Users();
			WinUser.Show();
			WinUser.Run ();
			WinUser.Destroy ();
			return;
		}

		//Загружаем информацию о пользователе
		if(QSMain.User.TestUserExistByLogin (true))
			QSMain.User.UpdateUserInfoByLogin ();
		UsersAction.Sensitive = QSMain.User.admin;
		labelUser.LabelProp = QSMain.User.Name;
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected void OnQuitActionActivated(object sender, EventArgs e)
	{
		Application.Quit();
	}

	protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
	{
		QSMain.User.ChangeUserPassword (this);
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

	protected void OnRunReferenceItemDialog(object sender, Reference.RunReferenceItemDlgEventArgs e)
	{
		ResponseType Result;
		/*switch (e.TableName)
		{
			case "lessees":
				lessee LesseeEdit = new lessee();
				if(!e.NewItem)
					LesseeEdit.Fill(e.ItemId);
				LesseeEdit.Show();
				Result = (ResponseType)LesseeEdit.Run();
				LesseeEdit.Destroy();
				break;
			case "organizations":
				Organization OrgEdit = new Organization();
				if(!e.NewItem)
					OrgEdit.Fill(e.ItemId);
				OrgEdit.Show();
				Result = (ResponseType)OrgEdit.Run();
				OrgEdit.Destroy();
				break;
			case "stead":
				Stead SteadEdit = new Stead();
				if(!e.NewItem)
					SteadEdit.Fill(e.ItemId);
				SteadEdit.Show();
				Result = (ResponseType)SteadEdit.Run();
				SteadEdit.Destroy();
				break;
			case "contract_types":
				ContractType contractTypeEdit = new ContractType();
				if(!e.NewItem)
					contractTypeEdit.Fill(e.ItemId);
				contractTypeEdit.Show();
				Result = (ResponseType)contractTypeEdit.Run();
				contractTypeEdit.Destroy();
				break;
			default:
				Result = ResponseType.None;
				break;
		}
		e.Result = Result;*/
	}

	protected void OnReferenceUpdate(object sender, QSMain.ReferenceUpdatedEventArgs e)
	{
		switch (e.ReferenceTable) {
		/*	case "place_types":
				ComboWorks.ComboFillReference(comboPlaceType,"place_types", ComboWorks.ListMode.WithAll);
				ComboWorks.ComboFillReference(comboContractPlaceT,"place_types", ComboWorks.ListMode.WithAll);
				break;
			case "organizations":
				ComboWorks.ComboFillReference(comboPlaceOrg,"organizations", ComboWorks.ListMode.WithAll);
				ComboWorks.ComboFillReference(comboContractOrg, "organizations", ComboWorks.ListMode.WithAll);
				break;
			case "contract_category":
				ComboWorks.ComboFillReference(comboContractCategory,"contract_category", ComboWorks.ListMode.WithAll);
				break;*/
		} 
	}
		
	protected void OnActionOrdersToggled(object sender, EventArgs e)
	{
		if (ActionOrders.Active)
		{
			uiIdOrders = this.UIManager.AddUiFromResource("Vodovoz.toolbars.orders.xml");
			this.UIManager.EnsureUpdate();
		}
		else if (uiIdOrders > 0)
		{
			this.UIManager.RemoveUi(uiIdOrders);
		}
	}

	protected void OnActionNewOrderActivated(object sender, EventArgs e)
	{
		throw new NotImplementedException();
	}

	protected void OnActionServicesToggled(object sender, EventArgs e)
	{
		if (ActionServices.Active)
		{
			uiIdServices = this.UIManager.AddUiFromResource("Vodovoz.toolbars.services.xml");
			this.UIManager.EnsureUpdate();
		} 
		else if(uiIdServices > 0)
		{
			this.UIManager.RemoveUi(uiIdServices);
		}
	}

}
