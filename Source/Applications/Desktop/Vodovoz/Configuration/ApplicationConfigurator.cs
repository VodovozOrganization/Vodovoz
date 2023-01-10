using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Gamma.Binding;
using Gamma.Utilities;
using NHibernate.AdoNet;
using NLog;
using QS.Banks.Domain;
using QS.BusinessCommon.Domain;
using QS.HistoryLog;
using QS.Print;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Repositories;
using QSBusinessCommon;
using QSDocTemplates;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Cash.CashTransfer;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.DocumentDialogs;
using Vodovoz.Dialogs.Employees;
using Vodovoz.Dialogs.Goods;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.StoredResources;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.ViewModels.Cash;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Store;
using Vodovoz.Views.Logistic;
using Vodovoz.Views.Users;
using VodovozInfrastructure.Configuration;

namespace Vodovoz.Configuration
{
    public class ApplicationConfigurator : IApplicationConfigurator
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const int connectionTimeoutSeconds = 120;

        public void ConfigureOrm()
        {
            logger.Debug("Конфигурация ORM...");

            //Увеличиваем таймаут если нужно
            var dbConnectionStringBuilder = new DbConnectionStringBuilder { ConnectionString = QSMain.ConnectionString };
            if(dbConnectionStringBuilder.TryGetValue("ConnectionTimeout", out var timeoutAsObject)
                && timeoutAsObject is string timeoutAsString
            ) {
                if(Int32.TryParse(timeoutAsString, out int timeout) && timeout != connectionTimeoutSeconds) {
                    dbConnectionStringBuilder["ConnectionTimeout"] = connectionTimeoutSeconds;
                    QSMain.ConnectionString = dbConnectionStringBuilder.ConnectionString;
                }
            }
            else {
                dbConnectionStringBuilder.Add("ConnectionTimeout", connectionTimeoutSeconds);
                QSMain.ConnectionString = dbConnectionStringBuilder.ConnectionString;
            }

            var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
                .Dialect<MySQL57SpatialExtendedDialect>()
                .ConnectionString(QSMain.ConnectionString)
                .AdoNetBatchSize(100)
                .Driver<LoggedMySqlClientDriver>();

            // Настройка ORM
            OrmConfig.ConfigureOrm(
                dbConfig,
                new[] {
                    Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
                    Assembly.GetAssembly(typeof(HibernateMapping.Organizations.OrganizationMap)),
                    Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
                    Assembly.GetAssembly(typeof(Bank)),
                    Assembly.GetAssembly(typeof(HistoryMain)),
                    Assembly.GetAssembly(typeof(QS.Attachments.Domain.Attachment)),
                    Assembly.GetAssembly(typeof(QS.Report.Domain.UserPrintSettings)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				},
				cnf => {
                    cnf.DataBaseIntegration(
                        dbi => {
                            dbi.BatchSize = 100;
                            dbi.Batcher<MySqlClientBatchingBatcherFactory>();
                        }
                    );
                }
            );

            logger.Debug("OK");
        }

        public void CreateApplicationConfig()
        {
            logger.Debug("Конфигурация маппингов диалогов, HistoryTrace, принтеров и ParentReference...");

            #region Dialogs mapping

            OrmMain.ClassMappingList = new List<IOrmObjectMapping> {
                //Простые справочники
                OrmObjectMapping<CullingCategory>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<Nationality>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<Citizenship>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<Manufacturer>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<EquipmentColors>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<User>.Create().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<UserSettings>.Create().Dialog<UserSettingsView>(),
                OrmObjectMapping<FuelType>.Create().Dialog<FuelTypeViewModel>().DefaultTableView()
					.SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<MovementWagon>.Create().Dialog<MovementWagonViewModel>().DefaultTableView()
                    .SearchColumn("Название", x => x.Name).End(),
                //Остальные справочники
                OrmObjectMapping<CarProxyDocument>.Create().Dialog<CarProxyDlg>(),
                OrmObjectMapping<M2ProxyDocument>.Create().Dialog<M2ProxyDlg>(),
                OrmObjectMapping<ProductGroup>.Create().Dialog<ProductGroupDlg>(),
                OrmObjectMapping<CommentTemplate>.Create().Dialog<CommentTemplateDlg>().DefaultTableView()
                    .SearchColumn("Шаблон комментария", x => x.Comment).End(),
                OrmObjectMapping<FineTemplate>.Create().Dialog<FineTemplateDlg>().DefaultTableView()
                    .SearchColumn("Шаблон комментария", x => x.Reason).End(),
                OrmObjectMapping<MeasurementUnits>.Create().Dialog<MeasurementUnitsDlg>().DefaultTableView()
                    .SearchColumn("ОКЕИ", x => x.OKEI).SearchColumn("Название", x => x.Name).Column("Точность", x => x.Digits.ToString())
                    .End(),
                OrmObjectMapping<Contact>.Create().Dialog<ContactDlg>()
                    .DefaultTableView().SearchColumn("Фамилия", x => x.Surname).SearchColumn("Имя", x => x.Name)
                    .SearchColumn("Отчество", x => x.Patronymic).End(),
                OrmObjectMapping<Order>.Create().Dialog<OrderDlg>().PopupMenu(OrderPopupMenu.GetPopupMenu),
                OrmObjectMapping<UndeliveredOrder>.Create().Dialog<UndeliveredOrderDlg>(),
                OrmObjectMapping<Organization>.Create().Dialog<OrganizationDlg>().DefaultTableView().Column("Код", x => x.Id.ToString())
                    .SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<ProductSpecification>.Create().Dialog<ProductSpecificationDlg>().DefaultTableView()
                    .SearchColumn("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<EquipmentKind>.Create().Dialog<EquipmentKindDlg>().DefaultTableView()
                    .Column("Название", equipmentKind => equipmentKind.Name).End(),
                //Связанное с клиентом
                OrmObjectMapping<Proxy>.Create().Dialog<ProxyDlg>()
                    .DefaultTableView().SearchColumn("Номер", x => x.Number).SearchColumn("С", x => x.StartDate.ToShortDateString())
                    .SearchColumn("По", x => x.ExpirationDate.ToShortDateString()).End(),
                OrmObjectMapping<PaidRentPackage>.Create().Dialog<PaidRentPackageDlg>()
                    .DefaultTableView().SearchColumn("Название", x => x.Name).Column("Вид оборудования", x => x.EquipmentKind.Name)
                    .SearchColumn("Цена в сутки", x => CurrencyWorks.GetShortCurrencyString(x.PriceDaily))
                    .SearchColumn("Цена в месяц", x => CurrencyWorks.GetShortCurrencyString(x.PriceMonthly)).End(),
                OrmObjectMapping<FreeRentPackage>.Create().Dialog<FreeRentPackageDlg>().DefaultTableView()
                    .SearchColumn("Название", x => x.Name).Column("Вид оборудования", x => x.EquipmentKind.Name).OrderAsc(x => x.Name)
                    .End(),
                OrmObjectMapping<Counterparty>.Create().Dialog<CounterpartyDlg>().DefaultTableView()
                    .SearchColumn("Название", x => x.FullName).End(),
                OrmObjectMapping<Tag>.Create().Dialog<TagDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<CounterpartyContract>.Create().Dialog<CounterpartyContractDlg>(),
                OrmObjectMapping<DocTemplate>.Create().Dialog<DocTemplateDlg>().DefaultTableView().SearchColumn("Название", x => x.Name)
                    .Column("Тип", x => x.TemplateType.GetEnumTitle()).End(),
                OrmObjectMapping<TransferOperationDocument>.Create().Dialog<TransferOperationDocumentDlg>(),
                //Справочники с фильтрами
                OrmObjectMapping<Equipment>.Create().Dialog<EquipmentDlg>().JournalFilter<EquipmentFilter>()
                    .DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Номенклатура", x => x.NomenclatureName)
                    .Column("Тип", x => x.Nomenclature.Kind.Name).SearchColumn("Серийный номер", x => x.Serial)
                    .Column("Дата последней обработки", x => x.LastServiceDate.ToShortDateString()).End(),
                //Логисткика
                OrmObjectMapping<RouteList>.Create().Dialog<RouteListCreateDlg>()
                    .DefaultTableView().SearchColumn("Номер", x => x.Id.ToString()).Column("Дата", x => x.Date.ToShortDateString())
                    .Column("Статус", x => x.Status.GetEnumTitle())
                    .SearchColumn("Водитель", x => String.Format("{0} - {1}", x.Driver.FullName, x.Car.Title)).End(),
                OrmObjectMapping<RouteColumn>.Create().DefaultTableView().Column("Код", x => x.Id.ToString())
                    .SearchColumn("Название", x => x.Name).End(),
                OrmObjectMapping<DeliveryShift>.Create().Dialog<DeliveryShiftDlg>().DefaultTableView().SearchColumn("Название", x => x.Name)
                    .SearchColumn("Диапазон времени", x => x.DeliveryTime).End(),
                OrmObjectMapping<DeliveryDaySchedule>.Create().Dialog<DeliveryDayScheduleDlg>().DefaultTableView()
                    .SearchColumn("Название", x => x.Name).End(),
                //Сервис
                OrmObjectMapping<ServiceClaim>.Create().Dialog<ServiceClaimDlg>().DefaultTableView().Column("Номер", x => x.Id.ToString())
                    .Column("Тип", x => x.ServiceClaimType.GetEnumTitle()).Column("Оборудование", x => x.Equipment.Title)
                    .Column("Подмена", x => x.ReplacementEquipment != null ? "Да" : "Нет")
                    .Column("Точка доставки", x => x.DeliveryPoint.Title).End(),
                //Касса
                OrmObjectMapping<Income>.Create().Dialog<CashIncomeDlg>(),
                OrmObjectMapping<ExpenseCategory>.Create().Dialog<ExpenseCategoryViewModel>().DefaultTableView()
                    .Column("Код", x => x.Id.ToString()).SearchColumn("Название", e => e.Name)
                    .Column("Тип документа", e => e.ExpenseDocumentType.GetEnumTitle())
                    .TreeConfig(new RecursiveTreeConfig<ExpenseCategory>(x => x.Parent, x => x.Childs)).End(),
                OrmObjectMapping<Expense>.Create().Dialog<CashExpenseDlg>(),
                OrmObjectMapping<AdvanceReport>.Create().Dialog<AdvanceReportDlg>(),
                OrmObjectMapping<Fine>.Create().Dialog<FineDlg>(),
                OrmObjectMapping<IncomeCashTransferDocument>.Create().Dialog<IncomeCashTransferDlg>(),
                OrmObjectMapping<CommonCashTransferDocument>.Create().Dialog<CommonCashTransferDlg>(),
                //Банкинг
                OrmObjectMapping<AccountIncome>.Create(),
                OrmObjectMapping<AccountExpense>.Create(),
                //Склад
                OrmObjectMapping<Warehouse>.Create().Dialog<WarehouseDlg>().DefaultTableView().Column("Название", w => w.Name)
                    .Column("В архиве", w => w.IsArchive ? "Да" : "").End(),
                OrmObjectMapping<RegradingOfGoodsTemplate>.Create().Dialog<RegradingOfGoodsTemplateDlg>().DefaultTableView()
                    .Column("Название", w => w.Name).End(),
                OrmObjectMapping<CarEventType>.Create().Dialog<CarEventTypeViewModel>()
            };

			#region Складские документы

			OrmMain.AddObjectDescription<IncomingWater>().Dialog<IncomingWaterDlg>();
            OrmMain.AddObjectDescription<WriteoffDocument>().Dialog<WriteoffDocumentDlg>();
            OrmMain.AddObjectDescription<InventoryDocument>().Dialog<InventoryDocumentDlg>();
            OrmMain.AddObjectDescription<ShiftChangeWarehouseDocument>().Dialog<ShiftChangeWarehouseDocumentDlg>();
            OrmMain.AddObjectDescription<RegradingOfGoodsDocument>().Dialog<RegradingOfGoodsDocumentDlg>();
            OrmMain.AddObjectDescription<SelfDeliveryDocument>().Dialog<SelfDeliveryDocumentDlg>();
            OrmMain.AddObjectDescription<CarLoadDocument>().Dialog<CarLoadDocumentDlg>();
            OrmMain.AddObjectDescription<CarUnloadDocument>().Dialog<CarUnloadDocumentDlg>();

            #endregion

            #region Goods

            OrmMain.AddObjectDescription<Folder1c>().Dialog<Folder1cDlg>()
                .DefaultTableView()
                .SearchColumn("Код 1С", x => x.Code1c)
                .SearchColumn("Название", x => x.Name)
                .TreeConfig(new RecursiveTreeConfig<Folder1c>(x => x.Parent, x => x.Childs)).End();

            #endregion

            #region Простые справочники

            OrmMain.AddObjectDescription<NonReturnReason>()
                .DefaultTableView()
                .SearchColumn("Код", x => x.Id.ToString())
                .SearchColumn("Название", x => x.Name)
                .End();
            OrmMain.AddObjectDescription<PaymentFrom>()
                .DefaultTableView()
                .SearchColumn("Код", x => x.Id.ToString())
                .SearchColumn("Название", x => x.Name)
                .End();
            OrmMain.AddObjectDescription<Post>()
                .DefaultTableView()
                .SearchColumn("Код", x => x.Id.ToString())
                .SearchColumn("Название", x => x.Name)
                .End();
            OrmMain.AddObjectDescription<SalesChannel>()
                .DefaultTableView()
                .SearchColumn("Код", x => x.Id.ToString())
                .SearchColumn("Название", x => x.Name)
                .End();

            #endregion

            #region неПростые справочники

            OrmMain.AddObjectDescription<Subdivision>().Dialog<SubdivisionDlg>().DefaultTableView().SearchColumn("Название", x => x.Name)
                .Column("Руководитель", x => x.Chief == null ? "" : x.Chief.ShortName).SearchColumn("Номер", x => x.Id.ToString())
                .TreeConfig(new RecursiveTreeConfig<Subdivision>(x => x.ParentSubdivision, x => x.ChildSubdivisions)).End();
            OrmMain.AddObjectDescription<TypeOfEntity>()
                .Dialog<TypeOfEntityDlg>()
                .DefaultTableView()
                .SearchColumn("Тип документа", x => TypeOfEntityRepository.GetEntityNameByString(x.Type))
                .SearchColumn("Название документа", x => x.CustomName)
                .SearchColumn("Код", x => x.Id.ToString())
                .Column("Активно", x => !x.IsActive ? "нет" : String.Empty)
                .SearchColumn("Имя класса", x => x.Type)
                .OrderAsc(x => x.CustomName)
                .End();
            OrmMain.AddObjectDescription<Trainee>().Dialog<TraineeDlg>().DefaultTableView()
                .Column("Код", x => x.Id.ToString())
                .SearchColumn("Ф.И.О.", x => x.FullName)
                .OrderAsc(x => x.LastName).OrderAsc(x => x.Name).OrderAsc(x => x.Patronymic)
                .End();
            OrmMain.AddObjectDescription<DeliveryPriceRule>().Dialog<DeliveryPriceRuleDlg>().DefaultTableView()
                .Column("< 19л б.", x => x.Water19LCount.ToString())
                .Column("< 6л б.", x => x.Water6LCount)
                .Column("< 1,5л б.", x => x.Water1500mlCount)
                .Column("< 0,6л б.", x => x.Water600mlCount)
                .Column("< 0,5л б.", x => x.Water500mlCount)
                .Column("Минимальная сумма заказа", x => x.OrderMinSumEShopGoods.ToString())
                .SearchColumn("Описание правила", x => x.Title)
                .End();
            OrmMain.AddObjectDescription<Certificate>().Dialog<CertificateDlg>().DefaultTableView()
                .SearchColumn("Имя", x => x.Name)
                .Column("Тип", x => x.TypeOfCertificate.GetEnumTitle())
                .SearchColumn("Номер", x => x.Id.ToString())
                .SearchColumn("Начало срока", x => x.StartDate.HasValue ? x.StartDate.Value.ToString("dd.MM.yyyy") : "Ошибка!")
                .SearchColumn("Окончание срока",
                    x => x.ExpirationDate.HasValue ? x.ExpirationDate.Value.ToString("dd.MM.yyyy") : "Бессрочно")
                .Column("Архивный?", x => x.IsArchive ? "Да" : string.Empty)
                .OrderAsc(x => x.IsArchive)
                .OrderAsc(x => x.Id)
                .End();
            OrmMain.AddObjectDescription<StoredResource>().Dialog<ImageLoaderDlg>().DefaultTableView()
                .SearchColumn("Номер", x => x.Id.ToString())
                .SearchColumn("Название", x => x.Name)
                .End();
            OrmMain.AddObjectDescription<DeliveryPointCategory>().Dialog<DeliveryPointCategoryDlg>().DefaultTableView()
                .SearchColumn("Код", x => x.Id.ToString())
                .SearchColumn("Название", x => x.Name)
                .Column("В архиве?", x => x.IsArchive ? "Да" : "Нет")
                .OrderAsc(x => x.Name)
                .End();
            OrmMain.AddObjectDescription<CounterpartyActivityKind>().Dialog<CounterpartyActivityKindDlg>().DefaultTableView()
                .SearchColumn("Код", x => x.Id.ToString())
                .SearchColumn("Название", x => x.Name)
                .End();

            #endregion

            OrmMain.ClassMappingList.AddRange(QSBanks.QSBanksMain.GetModuleMaping());

            #endregion

            HistoryMain.Enable();
            TemplatePrinter.InitPrinter();
            ImagePrinter.InitPrinter();

            logger.Debug("OK");
        }
    }
}
