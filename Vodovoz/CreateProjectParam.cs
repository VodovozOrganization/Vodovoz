using System;
using System.Collections.Generic;
using QSBusinessCommon;
using QSBusinessCommon.Domain;
using QSContacts;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
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
			OrmMain.ClassMappingList = new List<IOrmObjectMapping> {
				//Простые справочники
				OrmObjectMapping<CullingCategory>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Nationality>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Manufacturer>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<EquipmentColors>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Post>.Create().DefaultTableView().SearchColumn("Должность", x => x.Name).End(),
				OrmObjectMapping<CounterpartyStatus>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Significance>.Create().DefaultTableView().SearchColumn("Значимость клиента", x => x.Name).End(),
				OrmObjectMapping<User>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<LogisticsArea>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<FuelType>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliveryShift>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Остальные справочники
				OrmObjectMapping<CommentTemplate>.Create().Dialog<CommentTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Comment).End(),
				OrmObjectMapping<MeasurementUnits>.Create ().Dialog<MeasurementUnitsDlg>().DefaultTableView().SearchColumn("ОКЕИ", x => x.OKEI).SearchColumn("Название", x => x.Name).Column("Точность", x => x.Digits).End(),
				OrmObjectMapping<Contact>.Create().Dialog <ContactDlg>()
					.DefaultTableView().SearchColumn("Фамилия", x => x.Surname).SearchColumn("Имя", x => x.Name).SearchColumn("Отчество", x => x.Patronymic).End(),
				OrmObjectMapping<Car>.Create().Dialog<CarsDlg>()
					.DefaultTableView().SearchColumn("Модель а/м", x => x.Model).SearchColumn("Гос. номер", x => x.RegistrationNumber).SearchColumn("Водитель", x => x.DriverInfo).End(),
				OrmObjectMapping<Proxy>.Create().Dialog<ProxyDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Number).SearchColumn("С", x => x.StartDate).SearchColumn("По", x => x.ExpirationDate).End(),
				OrmObjectMapping<Order>.Create().Dialog <OrderDlg>().PopupMenu(OrderPopupMenu.GetPopupMenu),
				OrmObjectMapping<DeliveryPoint>.Create().Dialog<DeliveryPointDlg>(),
				OrmObjectMapping<PaidRentPackage>.Create().Dialog<PaidRentPackageDlg>()
					.DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Цена в сутки", x => x.PriceDailyString).SearchColumn("Цена в месяц", x => x.PriceMonthlyString).End(),
				OrmObjectMapping<FreeRentPackage>.Create().Dialog<FreeRentPackageDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<FreeRentAgreement>.Create().Dialog<AdditionalAgreementFreeRent>(),
				OrmObjectMapping<DailyRentAgreement>.Create().Dialog<AdditionalAgreementDailyRent>(),
				OrmObjectMapping<NonfreeRentAgreement>.Create().Dialog<AdditionalAgreementNonFreeRent>(),
				OrmObjectMapping<WaterSalesAgreement>.Create().Dialog<AdditionalAgreementWater>(),
				OrmObjectMapping<RepairAgreement>.Create().Dialog<AdditionalAgreementRepair>(),
				OrmObjectMapping<Counterparty>.Create().Dialog<CounterpartyDlg>().DefaultTableView().SearchColumn("Название", x => x.FullName).End(),
				OrmObjectMapping<CounterpartyContract>.Create().Dialog<CounterpartyContractDlg>(),
				OrmObjectMapping<Organization>.Create().Dialog<OrganizationDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliverySchedule>.Create().Dialog<DeliveryScheduleDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Время доставки", x => x.DeliveryTime).End(),
				OrmObjectMapping<ProductSpecification>.Create().Dialog<ProductSpecificationDlg>().DefaultTableView().SearchColumn("Код", x => x.Id).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<EquipmentType>.Create().Dialog<EquipmentTypeDlg>().DefaultTableView().Column("Название",equipmentType=>equipmentType.Name).End(),
				// Документы
				OrmObjectMapping<IncomingInvoice>.Create().Dialog<IncomingInvoiceDlg>(),
					OrmObjectMapping<IncomingWater>.Create().Dialog<IncomingWaterDlg>(),
					OrmObjectMapping<MovementDocument>.Create().Dialog<MovementDocumentDlg>(),
					OrmObjectMapping<WriteoffDocument>.Create().Dialog<WriteoffDocumentDlg>(),
				//Справочники с фильтрами
				OrmObjectMapping<Nomenclature>.Create().Dialog<NomenclatureDlg>().JournalFilter<NomenclatureFilter>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип", x => x.CategoryString).End(),
				OrmObjectMapping<Equipment>.Create().Dialog<EquipmentDlg>().JournalFilter<EquipmentFilter>()
					.DefaultTableView().SearchColumn("Номенклатура", x => x.NomenclatureName).Column("Тип", x => x.Type).SearchColumn("Серийный номер", x => x.Serial).Column("Дата последней обработки", x => x.LastServiceDateString).End(),
				OrmObjectMapping<Employee>.Create().Dialog<EmployeeDlg>().JournalFilter<EmployeeFilter>()
					.DefaultTableView().SearchColumn("Ф.И.О.", x => x.FullName).End(),
				//Логисткика
				OrmObjectMapping<RouteList>.Create().Dialog<RouteListCreateDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Id).Column("Дата", x => x.DateString).Column("Статус", x => x.StatusString).Column("Водитель", x => x.DriverInfo).End(),
				OrmObjectMapping<RouteColumn>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Сервис
				OrmObjectMapping<ServiceClaim>.Create().Dialog<ServiceClaimDlg>(),
				//Касса
				OrmObjectMapping<IncomeCategory>.Create ().EditPermision ("money_manage").DefaultTableView ().Column ("Название", e => e.Name).End (),
				OrmObjectMapping<ExpenseCategory>.Create ().Dialog<CashExpenseCategoryDlg>().EditPermision ("money_manage").DefaultTableView ().Column ("Название", e => e.Name).End (),
				OrmObjectMapping<Income>.Create ().Dialog<CashIncomeDlg> (),
				OrmObjectMapping<Expense>.Create ().Dialog<CashExpenseDlg> (),
				OrmObjectMapping<AdvanceReport>.Create ().Dialog<AdvanceReportDlg> (),
				//Банкинг
				OrmObjectMapping<AccountIncome>.Create (),
				OrmObjectMapping<AccountExpense>.Create (),
				//Склад
				OrmObjectMapping<Warehouse>.Create().Dialog<WarehouseDlg>().DefaultTableView().Column("Название", w=>w.Name).End()
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
			ParentReferenceConfig.AddActions (new ParentReferenceActions<Employee, QSBanks.Account> {
				AddNewChild = (c, a) => c.AddAccount (a)
			});
		}
	}
}
