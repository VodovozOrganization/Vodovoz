using System;
using System.Collections.Generic;
using QSContacts;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	partial class MainClass
	{
		static void CreateProjectParam ()
		{
			QSMain.ProjectPermission = new Dictionary<string, UserPermission> ();
			QSMain.ProjectPermission.Add ("max_loan_amount", new UserPermission ("max_loan_amount", "Установка максимального кредита",
				"Пользователь имеет права для установки максимальной суммы кредита."));
			QSMain.ProjectPermission.Add ("logistican", new UserPermission ("logistican", "Логист", "Пользователь является логистом."));
			QSMain.ProjectPermission.Add ("money_manage", new UserPermission ("money_manage", "Управление деньгами", "Пользователь имеет доступ к денежным операциям(касса и т.п.)."));
		}


		static void CreateBaseConfig ()
		{
			logger.Info ("Настройка параметров базы...");

			// Настройка ORM
			OrmMain.ConfigureOrm (QSMain.ConnectionString, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(MainClass)),
				System.Reflection.Assembly.GetAssembly (typeof(QSBanks.QSBanksMain)),
				System.Reflection.Assembly.GetAssembly (typeof(QSContacts.QSContactsMain))
			});
			OrmMain.ClassMappingList = new System.Collections.Generic.List<IOrmObjectMapping> {
				//Простые справочники
				new OrmObjectMapping<CullingCategory> (null, "{Vodovoz.Domain.CullingCategory} Name[Название];"),
				new OrmObjectMapping<Nationality> (null, "{Vodovoz.Domain.Nationality} Name[Название];"),
				new OrmObjectMapping<Manufacturer> (null, "{Vodovoz.Domain.Manufacturer} Name[Название];"),
				new OrmObjectMapping<EquipmentType> (null, "{Vodovoz.Domain.EquipmentType} Name[Название];"),
				new OrmObjectMapping<EquipmentColors> (null, "{Vodovoz.Domain.EquipmentColors} Id[Код]; Name[Название];"),
				new OrmObjectMapping<Post> (null, "{QSContacts.Post} Name[Должность];"),
				new OrmObjectMapping<CounterpartyStatus> (null, "{Vodovoz.Domain.CounterpartyStatus} Name[Название];"),
				new OrmObjectMapping<Significance> (null, "{Vodovoz.Domain.Significance} Name[Значимость клиента];"),
				new OrmObjectMapping<User> (null, "{Vodovoz.Domain.User} Name[Название];"),
				new OrmObjectMapping<LogisticsArea> (null, "{Vodovoz.Domain.LogisticsArea} Name[Название]"),
				new OrmObjectMapping<FuelType> (null, "{Vodovoz.Domain.Logistic.FuelType} Name[Название]"),
				new OrmObjectMapping<DeliveryShift> (null, "{Vodovoz.Domain.Logistic.DeliveryShift} Name[Название]"),
				new OrmObjectMapping<Warehouse> (null, "{Vodovoz.Domain.Store.Warehouse} Name[Название]"),
				//Остальные справочники
				new OrmObjectMapping<CommentTemplate> (typeof(CommentTemplateDlg), "{Vodovoz.Domain.CommentTemplate} Comment[Шаблон комментария];", new string[] { "Comment" }),
				new OrmObjectMapping<MeasurementUnits> (typeof(MeasurementUnitsDlg), "{Vodovoz.Domain.MeasurementUnits} OKEI[ОКЕИ]; Name[Название]; Digits[Точность];"),
				new OrmObjectMapping<Contact> (typeof(ContactDlg), "{Vodovoz.Domain.Contact} Surname[Фамилия]; Name[Имя]; Lastname[Отчество]; Post[Должность]", new string[] {
					"Surname",
					"Name",
					"Lastname",
					"Post"
				}),
				new OrmObjectMapping<Car> (typeof(CarsDlg), "{Vodovoz.Domain.Logistic.Car} Model[Модель а/м]; RegistrationNumber[Гос. номер]; DriverInfo[Водитель];", new string[] {
					"Model",
					"RegistrationNumber",
					"DriverInfo"
				}),
				new OrmObjectMapping<Proxy> (typeof(ProxyDlg), "{Vodovoz.Domain.Proxy} Number[Номер]; StartDate[С]; ExpirationDate[По];", new string[] { "Number" }),
				new OrmObjectMapping<Order> (typeof(OrderDlg), "{Vodovoz.Domain.Orders.Order} Id[Номер]; StatusString[Статус]; ClientString[Клиент];"),
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
				new OrmObjectMapping<DeliverySchedule> (typeof(DeliveryScheduleDlg), "{Vodovoz.Domain.DeliverySchedule} Name[Название]; DeliveryTime[Время доставки];"),
				new OrmObjectMapping<ProductSpecification> (typeof(ProductSpecificationDlg), "{Vodovoz.Domain.ProductSpecification} Id[Код]; Name[Название];"),
				// Документы
				new OrmObjectMapping<IncomingInvoice> (typeof(IncomingInvoiceDlg), "{Vodovoz.Domain.Documents.IncomingInvoice} Id[Номер];"),
				new OrmObjectMapping<IncomingWater> (typeof(IncomingWaterDlg), "{Vodovoz.Domain.Documents.IncomingWater} Id[Номер];"),
				new OrmObjectMapping<MovementDocument> (typeof(MovementDocumentDlg), "{Vodovoz.Domain.Documents.MovementDocument} Id[Номер];"),
				new OrmObjectMapping<WriteoffDocument> (typeof(WriteoffDocumentDlg), "{Vodovoz.Domain.Documents.WriteoffDocument} Id[Номер];"),
				//Справочники с фильтрами
				new OrmObjectMapping<Nomenclature> (typeof(NomenclatureDlg), typeof(NomenclatureFilter), "{Vodovoz.Domain.Nomenclature} Id[Код]; Name[Название]; CategoryString[Тип];", new string[] {
					"Name",
					"CategoryString"
				}),
				new OrmObjectMapping<Equipment> (typeof(EquipmentDlg), typeof(EquipmentFilter), "{Vodovoz.Domain.Equipment} NomenclatureName[Номенклатура]; Type[Тип]; Serial[Серийный номер]; LastServiceDateString[Дата последней обработки];", new string[] {
					"Serial",
					"Type",
					"NomenclatureName",
					"LastServiceDateString"
				}),
				new OrmObjectMapping<Employee> (typeof(EmployeeDlg), typeof(EmployeeFilter), "{Vodovoz.Domain.Employee} LastName[Фамилия]; Name[Имя]; Patronymic[Отчество];", new string[] {
					"Name",
					"LastName",
					"Patronymic"
				}),
				//Логисткика
				new OrmObjectMapping<RouteList> (typeof(RouteListCreateDlg), "{Vodovoz.Domain.Logistic.RouteList} Id[Номер]; DateString[Дата]; StatusString[Статус]; DriverInfo[Водитель];"),
				new OrmObjectMapping<RouteColumn> (null, "{Vodovoz.Domain.Logistic.RouteColumn} Name[Название];"),
				//Сервис
				new OrmObjectMapping<ServiceClaim> (typeof(ServiceClaimDlg), "{Vodovoz.Domain.Service.ServiceClaim} Id[Номер];"),
				//Касса
				OrmObjectMapping<IncomeCategory>.Create().EditPermision ("money_manage").SimpleDisplay ().Column ("Название", e => e.Name).End (),
				OrmObjectMapping<ExpenseCategory>.Create().EditPermision ("money_manage").SimpleDisplay ().Column ("Название", e => e.Name).End (),
				OrmObjectMapping<Income>.Create().Dialog<CashIncomeDlg> (),
				OrmObjectMapping<Expense>.Create().Dialog<CashExpenseDlg> (),
				OrmObjectMapping<AdvanceReport>.Create().Dialog<AdvanceReportDlg> (),
			};
			OrmMain.ClassMappingList.AddRange (QSBanks.QSBanksMain.GetModuleMaping ());
			OrmMain.ClassMappingList.AddRange (QSContactsMain.GetModuleMaping ());

			//Настройка ParentReference
			ParentReferenceConfig.AddActions (new ParentReferenceActions<Organization, QSBanks.Account> {
				AddNewChild = (o, a) => o.AddAccount (a)
			});
			ParentReferenceConfig.AddActions (new ParentReferenceActions<Counterparty, QSBanks.Account> {
				AddNewChild = (c, a) => c.AddAccount (a)
			});

		}
	}
}
