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
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public static Label StatusBarLabel;
		public static MainWindow MainWin;

		public static void Main(string[] args)
		{
			Application.Init();
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) 
			{
				logger.FatalException("Поймано не обработаное исключение.", (Exception) e.ExceptionObject);
				QSMain.ErrorMessage(MainWin, (Exception) e.ExceptionObject);
			};

			CreateProjectParam();
			// Создаем окно входа
			Login LoginDialog = new QSProjectsLib.Login ();
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
			LoginResult = (ResponseType) LoginDialog.Run();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			LoginDialog.Destroy();
			// Настройка ORM
			OrmMain.ConfigureOrm(QSMain.ConnectionString, new string[]{ "Vodovoz", "QSBanks", "QSContacts"});
			OrmMain.ClassMapingList = new System.Collections.Generic.List<QSOrmProject.OrmObjectMaping>
			{
				new OrmObjectMaping(typeof(Proxy), typeof(ProxyDlg), "{Vodovoz.Proxy} Number[Номер]; StartDate[С]; ExpirationDate[По];", new string[] {"Number"}),
				new OrmObjectMaping(typeof(DeliveryPoint), typeof(DeliveryPointDlg), "{Vodovoz.DeliveryPoint} Name[Название];"),
				new OrmObjectMaping(typeof(PaidRentPackage), typeof(PaidRentPackageDlg), "{Vodovoz.PaidRentPackage} Name[Название]; RentPeriodString[Период аренды];"),
				new OrmObjectMaping(typeof(FreeRentPackage), typeof(FreeRentPackageDlg), "{Vodovoz.FreeRentPackage} Name[Название];"),
				new OrmObjectMaping(typeof(FreeRentAgreement), typeof(AdditionalAgreementFreeRent), "{Vodovoz.FreeRentAgreement} AgreementNumber[Номер];", new string[] {"AgreementNumber"}),
				new OrmObjectMaping(typeof(NonfreeRentAgreement), typeof(AdditionalAgreementNonFreeRent), "{Vodovoz.NonfreeRentAgreement} AgreementNumber[Номер];", new string[] {"AgreementNumber"}),
				new OrmObjectMaping(typeof(WaterSalesAgreement), typeof(AdditionalAgreementWater), "{Vodovoz.WaterSalesAgreement} AgreementNumber[Номер];", new string[] {"AgreementNumber"}),
				new OrmObjectMaping(typeof(RepairAgreement), typeof(AdditionalAgreementRepair), "{Vodovoz.RepairAgreement} AgreementNumber[Номер];", new string[] {"AgreementNumber"}),
				new OrmObjectMaping(typeof(Post), null, "{QSContacts.Post} Name[Должность];"),
				new OrmObjectMaping(typeof(CounterpartyStatus), null, "{Vodovoz.CounterpartyStatus} Name[Название];"),
				new OrmObjectMaping(typeof(Significance), null, "{Vodovoz.Significance} Name[Значимость клиента];"),
				new OrmObjectMaping(typeof(Counterparty), typeof(CounterpartyDlg), "{Vodovoz.Counterparty} Name[Наименование];"),
				new OrmObjectMaping(typeof(User), null, "{Vodovoz.User} Name[Название];"),
				new OrmObjectMaping(typeof(Organization), typeof(OrganizationDlg), "{Vodovoz.Organization} Name[Название];"),
				new OrmObjectMaping(typeof(Nomenclature), typeof(NomenclatureDlg), "{Vodovoz.Nomenclature} Name[Название];"),
				new OrmObjectMaping(typeof(Nationality), null, "{Vodovoz.Nationality} Name[Название];"),
				new OrmObjectMaping(typeof(Manufacturer), null, "{Vodovoz.Manufacturer} Name[Название];"),
				new OrmObjectMaping(typeof(EquipmentType), null, "{Vodovoz.EquipmentType} Name[Название];"),
				new OrmObjectMaping(typeof(MeasurementUnits), null, "{Vodovoz.MeasurementUnits} Name[Название];"),
				new OrmObjectMaping(typeof(EquipmentColors), null, "{Vodovoz.EquipmentColors} Name[Название];"),
				new OrmObjectMaping(typeof(Employee), typeof(EmployeeDlg), "{Vodovoz.Employee} LastName[Фамилия]; Name[Имя]; Patronymic[Отчество];", new string[] {"Name", "LastName", "Patronymic"}),
				new OrmObjectMaping(typeof(Car), typeof(CarsDlg), "{Vodovoz.Car} Model[Модель а/м]; RegistrationNumber[Гос. номер]; DriverInfo[Водитель];", new string[] {"Model", "RegistrationNumber", "DriverInfo"}),
				new OrmObjectMaping(typeof(CustomerContact), typeof(QSContacts.ContactDlg), "{QSContacts.Contact} Surname[Фамилия]; Name[Имя]; Lastname[Отчество]; Post[Должность]", new string[] {"Surname", "Name", "Lastname", "Post"})
			};
			OrmMain.ClassMapingList.AddRange(QSBanks.QSBanksMain.GetModuleMaping());
			OrmMain.ClassMapingList.AddRange(QSContacts.QSContactsMain.GetModuleMaping());

			//Запускаем программу
			MainWin = new MainWindow ();
			QSMain.ErrorDlgParrent = MainWin;
			if(QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			Application.Run ();
		}

		public static void StatusMessage(string message)
		{
			if (StatusBarLabel == null)
				return;
			Application.Invoke(delegate(object sender, EventArgs e) {
				StatusBarLabel.LabelProp = message;
				while (GLib.MainContext.Pending())
				{
					Gtk.Main.Iteration();
				}
			});
		}
	}
}
