using System;
using System.Collections.Generic;
using Gamma.Binding;
using Gamma.Utilities;
using QSBusinessCommon;
using QSBusinessCommon.Domain;
using QSContacts;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
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
				OrmObjectMapping<UserSettings>.Create().Dialog<UserSettingsDlg>(),
				OrmObjectMapping<LogisticsArea>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<FuelType>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliveryShift>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<MovementWagon>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Остальные справочники
				OrmObjectMapping<CommentTemplate>.Create().Dialog<CommentTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Comment).End(),
				OrmObjectMapping<MeasurementUnits>.Create ().Dialog<MeasurementUnitsDlg>().DefaultTableView().SearchColumn("ОКЕИ", x => x.OKEI).SearchColumn("Название", x => x.Name).Column("Точность", x => x.Digits.ToString()).End(),
				OrmObjectMapping<Contact>.Create().Dialog <ContactDlg>()
					.DefaultTableView().SearchColumn("Фамилия", x => x.Surname).SearchColumn("Имя", x => x.Name).SearchColumn("Отчество", x => x.Patronymic).End(),
				OrmObjectMapping<Car>.Create().Dialog<CarsDlg>()
					.DefaultTableView().SearchColumn("Модель а/м", x => x.Model).SearchColumn("Гос. номер", x => x.RegistrationNumber).SearchColumn("Водитель", x => x.Driver != null ? x.Driver.FullName : String.Empty).End(),
				OrmObjectMapping<Order>.Create().Dialog <OrderDlg>().PopupMenu(OrderPopupMenu.GetPopupMenu),
				OrmObjectMapping<Organization>.Create().Dialog<OrganizationDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliverySchedule>.Create().Dialog<DeliveryScheduleDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Время доставки", x => x.DeliveryTime).End(),
				OrmObjectMapping<ProductSpecification>.Create().Dialog<ProductSpecificationDlg>().DefaultTableView().SearchColumn("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<EquipmentType>.Create().Dialog<EquipmentTypeDlg>().DefaultTableView().Column("Название",equipmentType=>equipmentType.Name).End(),
				//Связанное с клиентом
				OrmObjectMapping<Proxy>.Create().Dialog<ProxyDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Number).SearchColumn("С", x => x.StartDate.ToShortDateString()).SearchColumn("По", x => x.ExpirationDate.ToShortDateString()).End(),
				OrmObjectMapping<DeliveryPoint>.Create().Dialog<DeliveryPointDlg>(),
				OrmObjectMapping<PaidRentPackage>.Create().Dialog<PaidRentPackageDlg>()
					.DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип оборудования", x => x.EquipmentType.Name).SearchColumn("Цена в сутки", x => CurrencyWorks.GetShortCurrencyString (x.PriceDaily)).SearchColumn("Цена в месяц", x => CurrencyWorks.GetShortCurrencyString (x.PriceMonthly)).End(),
					OrmObjectMapping<FreeRentPackage>.Create().Dialog<FreeRentPackageDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип оборудования", x => x.EquipmentType.Name).End(),
				OrmObjectMapping<FreeRentAgreement>.Create().Dialog<FreeRentAgreementDlg>(),
				OrmObjectMapping<DailyRentAgreement>.Create().Dialog<DailyRentAgreementDlg>(),
				OrmObjectMapping<NonfreeRentAgreement>.Create().Dialog<NonFreeRentAgreementDlg>(),
				OrmObjectMapping<WaterSalesAgreement>.Create().Dialog<WaterAgreementDlg>(),
				OrmObjectMapping<RepairAgreement>.Create().Dialog<RepairAgreementDlg>(),
				OrmObjectMapping<Counterparty>.Create().Dialog<CounterpartyDlg>().DefaultTableView().SearchColumn("Название", x => x.FullName).End(),
				OrmObjectMapping<CounterpartyContract>.Create().Dialog<CounterpartyContractDlg>(),
				OrmObjectMapping<DocTemplate>.Create().Dialog<DocTemplateDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип", x => x.TemplateType.GetEnumTitle()).End(),
				// Документы
				OrmObjectMapping<IncomingInvoice>.Create().Dialog<IncomingInvoiceDlg>(),
					OrmObjectMapping<IncomingWater>.Create().Dialog<IncomingWaterDlg>(),
					OrmObjectMapping<MovementDocument>.Create().Dialog<MovementDocumentDlg>(),
					OrmObjectMapping<WriteoffDocument>.Create().Dialog<WriteoffDocumentDlg>(),
					OrmObjectMapping<InventoryDocument>.Create().Dialog<InventoryDocumentDlg>(),
					OrmObjectMapping<RegradingOfGoodsDocument>.Create().Dialog<RegradingOfGoodsDocumentDlg>(),
				//Справочники с фильтрами
				OrmObjectMapping<Nomenclature>.Create().Dialog<NomenclatureDlg>().JournalFilter<NomenclatureFilter>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип", x => x.CategoryString).End(),
				OrmObjectMapping<Equipment>.Create().Dialog<EquipmentDlg>().JournalFilter<EquipmentFilter>()
					.DefaultTableView().SearchColumn("Номенклатура", x => x.NomenclatureName).Column("Тип", x => x.Nomenclature.Type.Name).SearchColumn("Серийный номер", x => x.Serial).Column("Дата последней обработки", x => x.LastServiceDate.ToShortDateString ()).End(),
				OrmObjectMapping<Employee>.Create().Dialog<EmployeeDlg>().JournalFilter<EmployeeFilter>()
					.DefaultTableView().SearchColumn("Ф.И.О.", x => x.FullName).Column("Категория", x => x.Category.GetEnumTitle()).End(),
				//Логисткика
				OrmObjectMapping<RouteList>.Create().Dialog<RouteListCreateDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Id.ToString()).Column("Дата", x => x.Date.ToShortDateString()).Column("Статус", x => x.Status.GetEnumTitle ()).Column("Водитель", x => String.Format ("{0} - {1}", x.Driver.FullName, x.Car.Title)).End(),
				OrmObjectMapping<RouteColumn>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Сервис
				OrmObjectMapping<ServiceClaim>.Create().Dialog<ServiceClaimDlg>().DefaultTableView().Column("Номер", x => x.Id.ToString()).Column("Тип", x => x.ServiceClaimType.GetEnumTitle()).Column("Оборудование", x => x.Equipment.Title).Column("Подмена", x => x.ReplacementEquipment != null ? "Да" : "Нет").Column("Точка доставки", x => x.DeliveryPoint.Title).End(),
				//Касса
				OrmObjectMapping<IncomeCategory>.Create ().EditPermision ("money_manage").DefaultTableView ().Column ("Название", e => e.Name).End (),
				OrmObjectMapping<ExpenseCategory>.Create ().Dialog<CashExpenseCategoryDlg>().EditPermision ("money_manage").DefaultTableView ().SearchColumn ("Название", e => e.Name).TreeConfig(new RecursiveTreeConfig<ExpenseCategory>(x => x.Parent, x => x.Childs)).End (),
				OrmObjectMapping<Income>.Create ().Dialog<CashIncomeDlg> (),
				OrmObjectMapping<Expense>.Create ().Dialog<CashExpenseDlg> (),
				OrmObjectMapping<AdvanceReport>.Create ().Dialog<AdvanceReportDlg> (),
				//Банкинг
				OrmObjectMapping<AccountIncome>.Create (),
				OrmObjectMapping<AccountExpense>.Create (),
				//Склад
				OrmObjectMapping<Warehouse>.Create().Dialog<WarehouseDlg>().DefaultTableView().Column("Название", w=>w.Name).End(),
				OrmObjectMapping<RegradingOfGoodsTemplate>.Create().Dialog<RegradingOfGoodsTemplateDlg>().DefaultTableView().Column("Название", w=>w.Name).End()
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
