using System;
using System.Collections.Generic;
using Gamma.Binding;
using Gamma.Utilities;
using NHibernate.AdoNet;
using NHibernate.Cfg;
using QS.DomainModel;
using QS.DomainModel.Tracking;
using QSBusinessCommon;
using QSBusinessCommon.Domain;
using QSContacts;
using QSHistoryLog;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.DocumentDialogs;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
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
			QSMain.ProjectPermission.Add ("logistic_admin", new UserPermission("logistic_admin", "Логист- пересчет топлива в закрытых МЛ", "Пользователь может пересчитывать километраж в закрытых МЛ"));
			QSMain.ProjectPermission.Add ("logistic_changedeliverytime", new UserPermission("logistic_changedeliverytime", "Логистика. Изменение времени доставки при ведении МЛ", "Пользователь может изменять время доставки в диалоге ведения маршрутного листа"));
			QSMain.ProjectPermission.Add ("money_manage", new UserPermission ("money_manage", "Управление деньгами", "Пользователь имеет доступ к денежным операциям(касса и т.п.)."));
			QSMain.ProjectPermission.Add ("routelist_unclosing", new UserPermission ("routelist_unclosing", "Касса. Отмена закрытия маршрутных листов", "Пользователь может переводить маршрутные листы из статуса Закрыт в статус Сдается"));
			QSMain.ProjectPermission.Add ("store_manage", new UserPermission("store_manage", "Управление складом", "Пользователь имеет полный доступ к складу."));
			QSMain.ProjectPermission.Add ("store_worker", new UserPermission("store_worker", "Работник склада", "Пользователь имеет доступ к складу."));
			QSMain.ProjectPermission.Add ("store_production", new UserPermission("store_production", "Управление складом производство", ""));
			QSMain.ProjectPermission.Add ("store_vartemyagi", new UserPermission("store_vartemyagi", "Управление складом производство Вартемяги", ""));
			QSMain.ProjectPermission.Add ("store_office", new UserPermission("store_office", "Управление складом офис", ""));
			QSMain.ProjectPermission.Add ("store_equipment", new UserPermission("store_equipment", "Управление складом оборудование", ""));
			QSMain.ProjectPermission.Add ("can_delete", new UserPermission("can_delete", "Удаление заказов и маршрутных листов", "Пользователь может удалять заказы и маршрутные листы в журналах."));
			QSMain.ProjectPermission.Add ("can_delete_fines", new UserPermission("can_delete_fines", "Удаление штрафов", "Пользователь может удалять штрафы."));
			QSMain.ProjectPermission.Add ("can_close_orders", new UserPermission("can_close_orders", "Закрытие заказов", "Пользователь может закрывать заказы вручную.") );
			QSMain.ProjectPermission.Add ("can_edit_wage", new UserPermission("can_edit_wage", "Установка заработной платы ", "Пользователь может устанавливать тип заработной платы и ставку."));
			QSMain.ProjectPermission.Add("change_driver_wage", new UserPermission("change_driver_wage", "Изменение типа расчета ЗП в МЛ", "Пользователь может устанавливать для МЛ другой расчет заработной платы."));
			QSMain.ProjectPermission.Add("can_set_archive", new UserPermission("can_set_archive", "Установка галочки 'Архив'", "Пользователь может устанавливать галочку архивный."));
		}

		static void CreateBaseConfig()
		{
			logger.Info("Настройка параметров базы...");
			//Увеличиваем таймоут
			QSMain.ConnectionString += ";ConnectionTimeout=120";

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
			                                .Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
						 					.ConnectionString(QSMain.ConnectionString)
			                                .AdoNetBatchSize(100)
											.ShowSql()
											.FormatSql();

			// Настройка ORM
			OrmMain.ConfigureOrm (db_config, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
				System.Reflection.Assembly.GetAssembly (typeof(QSBanks.QSBanksMain)),
				System.Reflection.Assembly.GetAssembly (typeof(QSContacts.QSContactsMain)),
				System.Reflection.Assembly.GetAssembly (typeof(QSHistoryLog.HistoryMain))
			}, 
			                      (cnf) => cnf.DataBaseIntegration(
				                      dbi => { dbi.BatchSize = 100; dbi.Batcher<MySqlClientBatchingBatcherFactory>();}
				                     ));
			OrmMain.ClassMappingList = new List<IOrmObjectMapping> {
				//Простые справочники
				OrmObjectMapping<CullingCategory>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Nationality>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Subdivision>.Create().Dialog<SubdivisionDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<Manufacturer>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<EquipmentColors>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<User>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<UserSettings>.Create().Dialog<UserSettingsDlg>(),
				OrmObjectMapping<FuelType>.Create().Dialog<FuelTypeDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Стоимость", x => x.Cost.ToString()).End(),
				OrmObjectMapping<MovementWagon>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Остальные справочники
				OrmObjectMapping<DiscountReason>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<CommentTemplate>.Create().Dialog<CommentTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Comment).End(),
				OrmObjectMapping<FineTemplate>.Create().Dialog<FineTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Reason).End(),
				OrmObjectMapping<MeasurementUnits>.Create ().Dialog<MeasurementUnitsDlg>().DefaultTableView().SearchColumn("ОКЕИ", x => x.OKEI).SearchColumn("Название", x => x.Name).Column("Точность", x => x.Digits.ToString()).End(),
				OrmObjectMapping<Contact>.Create().Dialog <ContactDlg>()
					.DefaultTableView().SearchColumn("Фамилия", x => x.Surname).SearchColumn("Имя", x => x.Name).SearchColumn("Отчество", x => x.Patronymic).End(),
				OrmObjectMapping<Car>.Create().Dialog<CarsDlg>()
					.DefaultTableView().SearchColumn("Модель а/м", x => x.Model).SearchColumn("Гос. номер", x => x.RegistrationNumber).SearchColumn("Водитель", x => x.Driver != null ? x.Driver.FullName : String.Empty).End(),
				OrmObjectMapping<Order>.Create().Trace().Dialog <OrderDlg>().PopupMenu(OrderPopupMenu.GetPopupMenu),
					OrmObjectMapping<Organization>.Create().Dialog<OrganizationDlg>().DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliverySchedule>.Create().Dialog<DeliveryScheduleDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Время доставки", x => x.DeliveryTime).End(),
				OrmObjectMapping<ProductSpecification>.Create().Dialog<ProductSpecificationDlg>().DefaultTableView().SearchColumn("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<EquipmentType>.Create().Dialog<EquipmentTypeDlg>().DefaultTableView().Column("Название",equipmentType=>equipmentType.Name).End(),
				//Связанное с клиентом
				OrmObjectMapping<Proxy>.Create().Dialog<ProxyDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Number).SearchColumn("С", x => x.StartDate.ToShortDateString()).SearchColumn("По", x => x.ExpirationDate.ToShortDateString()).End(),
				OrmObjectMapping<DeliveryPoint>.Create().Dialog<DeliveryPointDlg>().DefaultTableView().SearchColumn("ID", x => x.Id.ToString()).Column("Адрес", x => x.Title).End(),
				OrmObjectMapping<PaidRentPackage>.Create().Dialog<PaidRentPackageDlg>()
					.DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип оборудования", x => x.EquipmentType.Name).SearchColumn("Цена в сутки", x => CurrencyWorks.GetShortCurrencyString (x.PriceDaily)).SearchColumn("Цена в месяц", x => CurrencyWorks.GetShortCurrencyString (x.PriceMonthly)).End(),
					OrmObjectMapping<FreeRentPackage>.Create().Dialog<FreeRentPackageDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип оборудования", x => x.EquipmentType.Name).OrderAsc(x => x.Name).End(),
				OrmObjectMapping<FreeRentAgreement>.Create().Dialog<FreeRentAgreementDlg>(),
				OrmObjectMapping<DailyRentAgreement>.Create().Dialog<DailyRentAgreementDlg>(),
				OrmObjectMapping<NonfreeRentAgreement>.Create().Dialog<NonFreeRentAgreementDlg>(),
				OrmObjectMapping<SalesEquipmentAgreement>.Create().Dialog<EquipSalesAgreementDlg>(),
				OrmObjectMapping<WaterSalesAgreement>.Create().Trace().Dialog<WaterAgreementDlg>(),
				OrmObjectMapping<RepairAgreement>.Create().Dialog<RepairAgreementDlg>(),
				OrmObjectMapping<Counterparty>.Create().Dialog<CounterpartyDlg>().DefaultTableView().SearchColumn("Название", x => x.FullName).End(),
				OrmObjectMapping<CounterpartyContract>.Create().Dialog<CounterpartyContractDlg>(),
				OrmObjectMapping<DocTemplate>.Create().Dialog<DocTemplateDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип", x => x.TemplateType.GetEnumTitle()).End(),
				OrmObjectMapping<Residue>.Create().Dialog<ResidueDlg>(),
				OrmObjectMapping<TransferOperationDocument>.Create().Dialog<TransferOperationDocumentDlg>(),
				// Документы
				OrmObjectMapping<IncomingInvoice>.Create().Dialog<IncomingInvoiceDlg>(),
					OrmObjectMapping<IncomingWater>.Create().Dialog<IncomingWaterDlg>(),
					OrmObjectMapping<MovementDocument>.Create().Dialog<MovementDocumentDlg>(),
					OrmObjectMapping<WriteoffDocument>.Create().Dialog<WriteoffDocumentDlg>(),
					OrmObjectMapping<InventoryDocument>.Create().Dialog<InventoryDocumentDlg>(),
					OrmObjectMapping<RegradingOfGoodsDocument>.Create().Dialog<RegradingOfGoodsDocumentDlg>(), 
					OrmObjectMapping<SelfDeliveryDocument>.Create().Dialog<SelfDeliveryDocumentDlg>(),
					OrmObjectMapping<CarLoadDocument>.Create().Dialog<CarLoadDocumentDlg>(),
					OrmObjectMapping<CarUnloadDocument>.Create().Dialog<CarUnloadDocumentDlg>(),
				//Справочники с фильтрами
				OrmObjectMapping<Nomenclature>.Create().Dialog<NomenclatureDlg>().JournalFilter<NomenclatureFilter>().DefaultTableView().SearchColumn("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).Column("Тип", x => x.CategoryString).End(),
				OrmObjectMapping<Equipment>.Create().Dialog<EquipmentDlg>().JournalFilter<EquipmentFilter>()
					.DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Номенклатура", x => x.NomenclatureName).Column("Тип", x => x.Nomenclature.Type.Name).SearchColumn("Серийный номер", x => x.Serial).Column("Дата последней обработки", x => x.LastServiceDate.ToShortDateString ()).End(),
				OrmObjectMapping<Employee>.Create().Dialog<EmployeeDlg>().JournalFilter<EmployeeFilter>()
					.DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Ф.И.О.", x => x.FullName).Column("Категория", x => x.Category.GetEnumTitle()).OrderAsc(x => x.LastName).OrderAsc(x => x.Name).OrderAsc(x => x.Patronymic).End(),
				//Логисткика
				OrmObjectMapping<RouteList>.Create().Dialog<RouteListCreateDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Id.ToString()).Column("Дата", x => x.Date.ToShortDateString()).Column("Статус", x => x.Status.GetEnumTitle ()).SearchColumn("Водитель", x => String.Format ("{0} - {1}", x.Driver.FullName, x.Car.Title)).End(),
				OrmObjectMapping<RouteColumn>.Create().DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliveryShift>.Create().Dialog<DeliveryShiftDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Диапазон времени", x => x.DeliveryTime).End(),
				OrmObjectMapping<DeliveryDaySchedule>.Create().Dialog<DeliveryDayScheduleDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<LogisticsArea>.Create().Dialog<LogisticsAreaDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Город", x => x.IsCity ? "Да" : "Нет").End(),
				//Сервис
				OrmObjectMapping<ServiceClaim>.Create().Dialog<ServiceClaimDlg>().DefaultTableView().Column("Номер", x => x.Id.ToString()).Column("Тип", x => x.ServiceClaimType.GetEnumTitle()).Column("Оборудование", x => x.Equipment.Title).Column("Подмена", x => x.ReplacementEquipment != null ? "Да" : "Нет").Column("Точка доставки", x => x.DeliveryPoint.Title).End(),
				//Касса
				OrmObjectMapping<IncomeCategory>.Create ().EditPermision ("money_manage").DefaultTableView ().Column("Код", x => x.Id.ToString()).Column ("Название", e => e.Name).End (),
				OrmObjectMapping<ExpenseCategory>.Create ().Dialog<CashExpenseCategoryDlg>().EditPermision ("money_manage").DefaultTableView ().Column("Код", x => x.Id.ToString()).SearchColumn ("Название", e => e.Name).TreeConfig(new RecursiveTreeConfig<ExpenseCategory>(x => x.Parent, x => x.Childs)).End (),
				OrmObjectMapping<Income>.Create ().Dialog<CashIncomeDlg> (),
				OrmObjectMapping<Expense>.Create ().Dialog<CashExpenseDlg> (),
				OrmObjectMapping<AdvanceReport>.Create ().Dialog<AdvanceReportDlg> (),
				OrmObjectMapping<Fine>.Create ().Dialog<FineDlg> (),
				//Банкинг
				OrmObjectMapping<AccountIncome>.Create (),
				OrmObjectMapping<AccountExpense>.Create (),
				//Склад
				OrmObjectMapping<Warehouse>.Create().Dialog<WarehouseDlg>().EditPermision("store_manage").DefaultTableView().Column("Название", w=>w.Name).End(),
				OrmObjectMapping<RegradingOfGoodsTemplate>.Create().Dialog<RegradingOfGoodsTemplateDlg>().EditPermision("store_manage").DefaultTableView().Column("Название", w=>w.Name).End()
			};

			OrmMain.ClassMappingList.AddRange (QSBanks.QSBanksMain.GetModuleMaping ());
			OrmMain.ClassMappingList.AddRange (QSContactsMain.GetModuleMaping ());

			HistoryMain.ConfigureFromOrmMain();

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

		public static void SetupAppFromBase()
		{
			//Устанавливаем код города по умолчанию.
			if (MainSupport.BaseParameters.All.ContainsKey ("default_city_code"))
				QSContactsMain.DefaultCityCode = MainSupport.BaseParameters.All ["default_city_code"];
		}
	}
}
