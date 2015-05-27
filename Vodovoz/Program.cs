using Gtk;
using NLog;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain;

namespace Vodovoz
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		public static void Main (string[] args)
		{
			Application.Init ();
			QSMain.SubscribeToUnhadledExceptions ();
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
				//Простые справочники
				new OrmObjectMapping (typeof(Nationality), null, "{Vodovoz.Domain.Nationality} Name[Название];"),
				new OrmObjectMapping (typeof(Manufacturer), null, "{Vodovoz.Domain.Manufacturer} Name[Название];"),
				new OrmObjectMapping (typeof(EquipmentType), null, "{Vodovoz.Domain.EquipmentType} Name[Название];"),
				new OrmObjectMapping (typeof(MeasurementUnits), null, "{Vodovoz.Domain.MeasurementUnits} Name[Название];"),
				new OrmObjectMapping (typeof(EquipmentColors), null, "{Vodovoz.Domain.EquipmentColors} Name[Название];"),
				new OrmObjectMapping (typeof(Post), null, "{QSContacts.Post} Name[Должность];"),
				new OrmObjectMapping (typeof(CounterpartyStatus), null, "{Vodovoz.Domain.CounterpartyStatus} Name[Название];"),
				new OrmObjectMapping (typeof(Significance), null, "{Vodovoz.Domain.Significance} Name[Значимость клиента];"),
				new OrmObjectMapping (typeof(User), null, "{Vodovoz.Domain.User} Name[Название];"),
				new OrmObjectMapping (typeof(LogisticsArea), null, "{Vodovoz.Domain.LogisticsArea} Name[Название]"),
				new OrmObjectMapping (typeof(Warehouse), null, "{Vodovoz.Domain.Warehouse} Name[Название]"),
				//Остальные справочники
				new OrmObjectMapping (typeof(Contact), typeof(ContactDlg), "{Vodovoz.Domain.Contact} Surname[Фамилия]; Name[Имя]; Lastname[Отчество]; Post[Должность]", new string[] { "Surname", "Name", "Lastname", "Post" }),
				new OrmObjectMapping (typeof(Car), typeof(CarsDlg), "{Vodovoz.Domain.Car} Model[Модель а/м]; RegistrationNumber[Гос. номер]; DriverInfo[Водитель];", new string[] { "Model", "RegistrationNumber", "DriverInfo" }),
				new OrmObjectMapping (typeof(Proxy), typeof(ProxyDlg), "{Vodovoz.Domain.Proxy} Number[Номер]; StartDate[С]; ExpirationDate[По];", new string[] { "Number" }),
				new OrmObjectMapping (typeof(DeliveryPoint), typeof(DeliveryPointDlg), "{Vodovoz.Domain.DeliveryPoint} Name[Название];"),
				new OrmObjectMapping (typeof(PaidRentPackage), typeof(PaidRentPackageDlg), "{Vodovoz.Domain.PaidRentPackage} Name[Название]; PriceDailyString[Цена в сутки]; PriceMonthlyString[Цена в месяц]; "),
				new OrmObjectMapping (typeof(FreeRentPackage), typeof(FreeRentPackageDlg), "{Vodovoz.Domain.FreeRentPackage} Name[Название];"),
				new OrmObjectMapping (typeof(FreeRentAgreement), typeof(AdditionalAgreementFreeRent), "{Vodovoz.Domain.FreeRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(DailyRentAgreement), typeof(AdditionalAgreementDailyRent), "{Vodovoz.Domain.DailyRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(NonfreeRentAgreement), typeof(AdditionalAgreementNonFreeRent), "{Vodovoz.Domain.NonfreeRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(WaterSalesAgreement), typeof(AdditionalAgreementWater), "{Vodovoz.Domain.WaterSalesAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(RepairAgreement), typeof(AdditionalAgreementRepair), "{Vodovoz.Domain.RepairAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping (typeof(Counterparty), typeof(CounterpartyDlg), "{Vodovoz.Domain.Counterparty} Name[Наименование];"),
				new OrmObjectMapping (typeof(CounterpartyContract), typeof(CounterpartyContractDlg), "{Vodovoz.Domain.CounterpartyContract} Number[Номер договора];"),
				new OrmObjectMapping (typeof(Organization), typeof(OrganizationDlg), "{Vodovoz.Domain.Organization} Name[Название];"),
				new OrmObjectMapping (typeof(PaidRentEquipment), typeof(PaidRentEquipmentDlg), "{Vodovoz.Domain.PaidRentEquipment} PackageName[Пакет]; EquipmentName[Название];"),
				new OrmObjectMapping (typeof(FreeRentEquipment), typeof(FreeRentEquipmentDlg), "{Vodovoz.Domain.FreeRentEquipment} PackageName[Пакет]; EquipmentName[Название];"),
				new OrmObjectMapping (typeof(DeliverySchedule), typeof(DeliveryScheduleDlg), "{Vodovoz.Domain.DeliverySchedule} Name[Название]; DeliveryTime[Время доставки];"),
				new OrmObjectMapping (typeof(IncomingInvoice), typeof(IncomingInvoiceDlg), "{Vodovoz.Domain.IncomingInvoice} Id[Номер];"),
				new OrmObjectMapping (typeof(IncomingWater), typeof(IncomingWaterDlg), "{Vodovoz.Domain.IncomingWater} Id[Номер];"),
				new OrmObjectMapping (typeof(MovementDocument), typeof(MovementDocumentDlg), "{Vodovoz.Domain.MovementDocument} Id[Номер];"),
				new OrmObjectMapping (typeof(WriteoffDocument), typeof(WriteoffDocumentDlg), "{Vodovoz.Domain.WriteoffDocument} Id[Номер];"),
				new OrmObjectMapping (typeof(IncomingInvoiceItem), typeof(AddInvoiceItemDlg), "{Vodovoz.Domain.IncomingInvoiceItem} Id[Номер];"),
				//Справочники с фильтрами
				new OrmObjectMapping (typeof(Nomenclature), typeof(NomenclatureDlg), typeof(NomenclatureFilter), "{Vodovoz.Domain.Nomenclature} Name[Название]; CategoryString[Тип];", new string[] { "Name", "CategoryString" }),
				new OrmObjectMapping (typeof(Equipment), typeof(EquipmentDlg), typeof(EquipmentFilter), "{Vodovoz.Domain.Equipment} NomenclatureName[Номенклатура]; Type[Тип]; Serial[Серийный номер]; LastServiceDateString[Дата последней обработки];", new string[] { "Serial", "Type", "NomenclatureName", "LastServiceDateString" }),
				new OrmObjectMapping (typeof(Employee), typeof(EmployeeDlg), typeof(EmployeeFilter), "{Vodovoz.Domain.Employee} LastName[Фамилия]; Name[Имя]; Patronymic[Отчество];", new string[] { "Name", "LastName", "Patronymic" })
			};
			OrmMain.ClassMappingList.AddRange (QSBanks.QSBanksMain.GetModuleMaping ());
			OrmMain.ClassMappingList.AddRange (QSContactsMain.GetModuleMaping ());

			//Настрока удаления
			ConfigureDeletion ();

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
