using System;
using Gtk;
using QSProjectsLib;
using QSOrmProject;
using NLog;
using QSContacts;

namespace Vodovoz
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		public static void Main (string[] args)
		{
			Application.Init ();
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
				logger.FatalException ("Поймано не обработаное исключение.", (Exception)e.ExceptionObject);
				QSMain.ErrorMessage (MainWin, (Exception)e.ExceptionObject);
			};
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			CreateProjectParam ();
			// Создаем окно входа
			Login LoginDialog = new Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("Vodovoz.icons.logo.png");
			LoginDialog.SetDefaultNames ("Vodovoz");
			LoginDialog.DefaultLogin = "user";
			LoginDialog.DefaultServer = "vod-srv.qsolution.ru";
			LoginDialog.DemoServer = "demo.qsolution.ru";
			LoginDialog.DemoMessage = "Для подключения к демострационному серверу используйте следующие настройки:\n" +
			"\n" +
			"<b>Сервер:</b> demo.qsolution.ru\n" +
			"<b>Пользователь:</b> demo\n" +
			"<b>Пароль:</b> demo\n" +
			"\n" +
			"Для установки собственного сервера обратитесь к документации.";
			LoginDialog.UpdateFromGConf ();

			ResponseType LoginResult;
			LoginResult = (ResponseType)LoginDialog.Run ();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			LoginDialog.Destroy ();
			// Настройка ORM
			OrmMain.ConfigureOrm (QSMain.ConnectionString, new string[]{ "Vodovoz", "QSBanks", "QSContacts" });
			OrmMain.ClassMappingList = new System.Collections.Generic.List<OrmObjectMapping> {
				new OrmObjectMapping (typeof(Proxy), typeof(ProxyDlg), "{Vodovoz.Proxy} Number[Номер]; StartDate[С]; ExpirationDate[По];", new string[] { "Number" }),
				new OrmObjectMapping (typeof(DeliveryPoint), typeof(DeliveryPointDlg), "{Vodovoz.DeliveryPoint} Name[Название];"),
				new OrmObjectMapping (typeof(PaidRentPackage), typeof(PaidRentPackageDlg), "{Vodovoz.PaidRentPackage} Name[Название]; RentPeriodString[Период аренды];"),
				new OrmObjectMapping (typeof(FreeRentPackage), typeof(FreeRentPackageDlg), "{Vodovoz.FreeRentPackage} Name[Название];"),
				new OrmObjectMapping (typeof(FreeRentAgreement), typeof(AdditionalAgreementFreeRent), "{Vodovoz.FreeRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(NonfreeRentAgreement), typeof(AdditionalAgreementNonFreeRent), "{Vodovoz.NonfreeRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(WaterSalesAgreement), typeof(AdditionalAgreementWater), "{Vodovoz.WaterSalesAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(RepairAgreement), typeof(AdditionalAgreementRepair), "{Vodovoz.RepairAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(Post), null, "{QSContacts.Post} Name[Должность];"),
				new OrmObjectMapping (typeof(CounterpartyStatus), null, "{Vodovoz.CounterpartyStatus} Name[Название];"),
				new OrmObjectMapping (typeof(Significance), null, "{Vodovoz.Significance} Name[Значимость клиента];"),
				new OrmObjectMapping (typeof(Counterparty), typeof(CounterpartyDlg), "{Vodovoz.Counterparty} Name[Наименование];"),
				new OrmObjectMapping (typeof(User), null, "{Vodovoz.User} Name[Название];"),
				new OrmObjectMapping (typeof(Organization), typeof(OrganizationDlg), "{Vodovoz.Organization} Name[Название];"),
				new OrmObjectMapping (typeof(Nomenclature), typeof(NomenclatureDlg), "{Vodovoz.Nomenclature} Name[Название];"),
				new OrmObjectMapping (typeof(Nationality), null, "{Vodovoz.Nationality} Name[Название];"),
				new OrmObjectMapping (typeof(Manufacturer), null, "{Vodovoz.Manufacturer} Name[Название];"),
				new OrmObjectMapping (typeof(EquipmentType), null, "{Vodovoz.EquipmentType} Name[Название];"),
				new OrmObjectMapping (typeof(MeasurementUnits), null, "{Vodovoz.MeasurementUnits} Name[Название];"),
				new OrmObjectMapping (typeof(EquipmentColors), null, "{Vodovoz.EquipmentColors} Name[Название];"),
				new OrmObjectMapping (typeof(Employee), typeof(EmployeeDlg), typeof(EmployeeFilter), "{Vodovoz.Employee} LastName[Фамилия]; Name[Имя]; Patronymic[Отчество];", new string[] { "Name", "LastName", "Patronymic" }),
				new OrmObjectMapping (typeof(Car), typeof(CarsDlg), "{Vodovoz.Car} Model[Модель а/м]; RegistrationNumber[Гос. номер]; DriverInfo[Водитель];", new string[] { "Model", "RegistrationNumber", "DriverInfo" }),
				new OrmObjectMapping (typeof(CustomerContact), typeof(ContactDlg), "{QSContacts.Contact} Surname[Фамилия]; Name[Имя]; Lastname[Отчество]; Post[Должность]", new string[] { "Surname", "Name", "Lastname", "Post" }),
				new OrmObjectMapping (typeof(Equipment), typeof(EquipmentDlg), typeof(EquipmentFilter), "{Vodovoz.Equipment} NomenclatureName[Номенклатура]; Type[Тип]; Serial[Серийный номер]; LastServiceDateString[Дата последней обработки];", new string[] { "Serial", "Type", "NomenclatureName", "LastServiceDateString" }),
				new OrmObjectMapping (typeof(FreeRentEquipment), typeof(FreeRentEquipmentDlg), "{Vodovoz.FreeRentEquipment} PackageName[Пакет]; EquipmentName[Название];")
			};
			OrmMain.ClassMappingList.AddRange (QSBanks.QSBanksMain.GetModuleMaping ());
			OrmMain.ClassMappingList.AddRange (QSContactsMain.GetModuleMaping ());

			//Запускаем программу
			MainWin = new MainWindow ();
			QSMain.ErrorDlgParrent = MainWin;
			if (QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			QSSaaS.Session.StartSessionRefresh ();
			Application.Run ();
			QSSaaS.Session.StopSessionRefresh ();
		}
	}
}
