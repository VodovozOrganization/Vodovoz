using System;
using Gtk;
using NLog;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		[STAThread]
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
			OrmMain.ClassMappingList = new System.Collections.Generic.List<IOrmObjectMapping> {
				//Простые справочники
				new OrmObjectMapping<CullingCategory> (null, "{Vodovoz.Domain.CullingCategory} Name[Название];"),
				new OrmObjectMapping<Nationality> (null, "{Vodovoz.Domain.Nationality} Name[Название];"),
				new OrmObjectMapping<Manufacturer> (null, "{Vodovoz.Domain.Manufacturer} Name[Название];"),
				new OrmObjectMapping<EquipmentType> (null, "{Vodovoz.Domain.EquipmentType} Name[Название];"),
				new OrmObjectMapping<MeasurementUnits> (null, "{Vodovoz.Domain.MeasurementUnits} Name[Название];"),
				new OrmObjectMapping<EquipmentColors> (null, "{Vodovoz.Domain.EquipmentColors} Name[Название];"),
				new OrmObjectMapping<Post> (null, "{QSContacts.Post} Name[Должность];"),
				new OrmObjectMapping<CounterpartyStatus> (null, "{Vodovoz.Domain.CounterpartyStatus} Name[Название];"),
				new OrmObjectMapping<Significance> (null, "{Vodovoz.Domain.Significance} Name[Значимость клиента];"),
				new OrmObjectMapping<User> (null, "{Vodovoz.Domain.User} Name[Название];"),
				new OrmObjectMapping<LogisticsArea> (null, "{Vodovoz.Domain.LogisticsArea} Name[Название]"),
				new OrmObjectMapping<Warehouse> (null, "{Vodovoz.Domain.Warehouse} Name[Название]"),
				//Остальные справочники
				new OrmObjectMapping<Contact> (typeof(ContactDlg), "{Vodovoz.Domain.Contact} Surname[Фамилия]; Name[Имя]; Lastname[Отчество]; Post[Должность]", new string[] { "Surname", "Name", "Lastname", "Post" }),
				new OrmObjectMapping<Car> (typeof(CarsDlg), "{Vodovoz.Domain.Car} Model[Модель а/м]; RegistrationNumber[Гос. номер]; DriverInfo[Водитель];", new string[] { "Model", "RegistrationNumber", "DriverInfo" }),
				new OrmObjectMapping<Proxy> (typeof(ProxyDlg), "{Vodovoz.Domain.Proxy} Number[Номер]; StartDate[С]; ExpirationDate[По];", new string[] { "Number" }),
				new OrmObjectMapping<Order> (typeof(OrderDlg), "{Vodovoz.Domain.Order} Id[Номер];"),
				new OrmObjectMapping<DeliveryPoint> (typeof(DeliveryPointDlg), "{Vodovoz.Domain.DeliveryPoint} Name[Название];"),
				new OrmObjectMapping<PaidRentPackage> (typeof(PaidRentPackageDlg), "{Vodovoz.Domain.PaidRentPackage} Name[Название]; PriceDailyString[Цена в сутки]; PriceMonthlyString[Цена в месяц]; "),
				new OrmObjectMapping<FreeRentPackage> (typeof(FreeRentPackageDlg), "{Vodovoz.Domain.FreeRentPackage} Name[Название];"),
				new OrmObjectMapping<FreeRentAgreement> (typeof(AdditionalAgreementFreeRent), "{Vodovoz.Domain.FreeRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping<DailyRentAgreement> (typeof(AdditionalAgreementDailyRent), "{Vodovoz.Domain.DailyRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping<NonfreeRentAgreement> (typeof(AdditionalAgreementNonFreeRent), "{Vodovoz.Domain.NonfreeRentAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping<WaterSalesAgreement> (typeof(AdditionalAgreementWater), "{Vodovoz.Domain.WaterSalesAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping<RepairAgreement> (typeof(AdditionalAgreementRepair), "{Vodovoz.Domain.RepairAgreement} AgreementNumber[Номер];", new string[] { "AgreementNumber" }),
				new OrmObjectMapping<Counterparty> (typeof(CounterpartyDlg), "{Vodovoz.Domain.Counterparty} Name[Наименование];"),
				new OrmObjectMapping<CounterpartyContract> (typeof(CounterpartyContractDlg), "{Vodovoz.Domain.CounterpartyContract} Number[Номер договора];"),
				new OrmObjectMapping<Organization> (typeof(OrganizationDlg), "{Vodovoz.Domain.Organization} Name[Название];"),
				new OrmObjectMapping<PaidRentEquipment> (typeof(PaidRentEquipmentDlg), "{Vodovoz.Domain.PaidRentEquipment} PackageName[Пакет]; EquipmentName[Название];"),
				new OrmObjectMapping<FreeRentEquipment> (typeof(FreeRentEquipmentDlg), "{Vodovoz.Domain.FreeRentEquipment} PackageName[Пакет]; EquipmentName[Название];"),
				new OrmObjectMapping<DeliverySchedule> (typeof(DeliveryScheduleDlg), "{Vodovoz.Domain.DeliverySchedule} Name[Название]; DeliveryTime[Время доставки];"),
				new OrmObjectMapping<ProductSpecification> (typeof(ProductSpecificationDlg), "{Vodovoz.Domain.ProductSpecification} Id[Код]; Name[Название];"),
				// Документы
				new OrmObjectMapping<IncomingInvoice> (typeof(IncomingInvoiceDlg), "{Vodovoz.Domain.Documents.IncomingInvoice} Id[Номер];"),
				new OrmObjectMapping<IncomingWater> (typeof(IncomingWaterDlg), "{Vodovoz.Domain.Documents.IncomingWater} Id[Номер];"),
				new OrmObjectMapping<MovementDocument> (typeof(MovementDocumentDlg), "{Vodovoz.Domain.Documents.MovementDocument} Id[Номер];"),
				new OrmObjectMapping<WriteoffDocument> (typeof(WriteoffDocumentDlg), "{Vodovoz.Domain.Documents.WriteoffDocument} Id[Номер];"),
				//Справочники с фильтрами
				new OrmObjectMapping<Nomenclature> (typeof(NomenclatureDlg), typeof(NomenclatureFilter), "{Vodovoz.Domain.Nomenclature} Id[Код]; Name[Название]; CategoryString[Тип];", new string[] { "Name", "CategoryString" }),
				new OrmObjectMapping<Equipment> (typeof(EquipmentDlg), typeof(EquipmentFilter), "{Vodovoz.Domain.Equipment} NomenclatureName[Номенклатура]; Type[Тип]; Serial[Серийный номер]; LastServiceDateString[Дата последней обработки];", new string[] { "Serial", "Type", "NomenclatureName", "LastServiceDateString" }),
				new OrmObjectMapping<Employee> (typeof(EmployeeDlg), typeof(EmployeeFilter), "{Vodovoz.Domain.Employee} LastName[Фамилия]; Name[Имя]; Patronymic[Отчество];", new string[] { "Name", "LastName", "Patronymic" })
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
