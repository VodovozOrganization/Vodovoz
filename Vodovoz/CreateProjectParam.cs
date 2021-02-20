using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Autofac;
using Gamma.Binding;
using Gamma.Utilities;
using NHibernate.AdoNet;
using QS.Banks.Domain;
using QS.BusinessCommon.Domain;
using QS.Deletion.Views;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Navigation;
using QS.Permissions;
using QS.Print;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.Project.Services.GtkUI;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Resolve;
using QS.Views.Resolve;
using QS.Widgets.GtkUI;
using QSBusinessCommon;
using QSDocTemplates;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using QSReport;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Cash;
using Vodovoz.Dialogs.Cash.CashTransfer;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.DocumentDialogs;
using Vodovoz.Dialogs.Email;
using Vodovoz.Dialogs.Employees;
using Vodovoz.Dialogs.Fuel;
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
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.StoredResources;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.GtkViews;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Footers.ViewModels;
using Vodovoz.Footers.Views;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.JournalViewers;
using Vodovoz.PermissionExtensions;
using Vodovoz.ReportsParameters;
using Vodovoz.Services.Permissions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.ViewModels.Organization;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Suppliers;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewModels.WageCalculation;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Views;
using Vodovoz.Views.BusinessTasks;
using Vodovoz.Views.Cash;
using Vodovoz.Views.Complaints;
using Vodovoz.Views.Contacts;
using Vodovoz.Views.Employees;
using Vodovoz.Views.Logistic;
using Vodovoz.Views.Orders;
using Vodovoz.Views.Orders.OrdersWithoutShipment;
using Vodovoz.Views.Organization;
using Vodovoz.Views.Suppliers;
using Vodovoz.Views.Users;
using Vodovoz.Views.WageCalculation;
using Vodovoz.Views.Warehouse;
using Vodovoz.ViewWidgets.AdvancedWageParameterViews;
using Vodovoz.ViewWidgets.Permissions;
using Vodovoz.ViewWidgets.PromoSetAction;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;
using Vodovoz.Views.Goods;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Organizations;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Tools;
using Vodovoz.Views.Mango.Talks;
using Vodovoz.ViewModels.Mango.Talks;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Store;
using Vodovoz.Views.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.JournalFilters.Proposal;
using Vodovoz.ViewModels.ViewModels.Proposal;
using Vodovoz.Views.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Security;
using Vodovoz.Views.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;

namespace Vodovoz
{
	partial class MainClass
	{
		static void CreateProjectParam()
		{
			UserDialog.RequestWidth = 900;
			UserDialog.RequestHeight = 700;

			UserDialog.UserPermissionViewsCreator = () => new List<IUserPermissionTab> {
				new SubdivisionForUserEntityPermissionWidget(),
				new PresetPermissionsView()
			};

			UserDialog.PermissionViewsCreator = () => new List<IPermissionsView>
			{
				new PermissionMatrixView(
					new PermissionMatrix<WarehousePermissions, Warehouse>(), "Доступ к складам", "warehouse_access")
			};

			WarehousePermissionService.WarehousePermissionValidatorFactory = new WarehousePermissionValidatorFactory();
		}

		static void ConfigureViewModelWidgetResolver()
		{
			ViewModelWidgetResolver.Instance = new BasedOnNameViewModelWidgetResolver();

			//Регистрация вкладок
			ViewModelWidgetResolver.Instance
				.RegisterWidgetForTabViewModel<DistrictViewModel, DistrictView>()
				.RegisterWidgetForTabViewModel<DistrictsSetActivationViewModel, DistrictsSetActivationView>()
				.RegisterWidgetForTabViewModel<FuelTransferDocumentViewModel, FuelTransferDocumentView>()
				.RegisterWidgetForTabViewModel<FuelIncomeInvoiceViewModel, FuelIncomeInvoiceView>()
				.RegisterWidgetForTabViewModel<ClientCameFromViewModel, ClientCameFromView>()
				.RegisterWidgetForTabViewModel<FuelTypeViewModel, FuelTypeView>()
				.RegisterWidgetForTabViewModel<FuelWriteoffDocumentViewModel, FuelWriteoffDocumentView>()
				.RegisterWidgetForTabViewModel<ResidueViewModel, ResidueView>()
				.RegisterWidgetForTabViewModel<FineTemplateViewModel, FineTemplateView>()
				.RegisterWidgetForTabViewModel<ComplaintViewModel, ComplaintView>()
				.RegisterWidgetForTabViewModel<IncomeCategoryViewModel, IncomeCategoryView>()
				.RegisterWidgetForTabViewModel<ExpenseCategoryViewModel, ExpenseCategoryView>()
				.RegisterWidgetForTabViewModel<CashRequestViewModel, CashRequestView>()
				.RegisterWidgetForTabViewModel<CashRequestItemViewModel, CashRequestItemView>()
				.RegisterWidgetForTabViewModel<CreateComplaintViewModel, CreateComplaintView>()
				.RegisterWidgetForTabViewModel<CreateInnerComplaintViewModel, CreateInnerComplaintView>()
				.RegisterWidgetForTabViewModel<ComplaintSourceViewModel, ComplaintSourceView>()
				.RegisterWidgetForTabViewModel<ComplaintResultViewModel, ComplaintResultView>()
				.RegisterWidgetForTabViewModel<SubdivisionViewModel, SubdivisionView>()
				.RegisterWidgetForTabViewModel<FineViewModel, FineView>()
				.RegisterWidgetForTabViewModel<RequestToSupplierViewModel, RequestToSupplierView>()
				.RegisterWidgetForTabViewModel<WageDistrictViewModel, WageDistrictView>()
				.RegisterWidgetForTabViewModel<WageDistrictLevelRatesViewModel, WageDistrictLevelRatesView>()
				.RegisterWidgetForTabViewModel<EmployeeWageParameterViewModel, WageParameterView>()
				.RegisterWidgetForTabViewModel<SalesPlanViewModel, SalesPlanView>()
				.RegisterWidgetForTabViewModel<RouteListsOnDayViewModel, RouteListsOnDayView>()
				.RegisterWidgetForTabViewModel<FuelDocumentViewModel, FuelDocumentView>()
				.RegisterWidgetForTabViewModel<ComplaintKindViewModel, ComplaintKindView>()
				.RegisterWidgetForTabViewModel<MovementDocumentViewModel, MovementDocumentView>()
				.RegisterWidgetForTabViewModel<IncomingInvoiceViewModel, IncomingInvoiceView>()
				.RegisterWidgetForTabViewModel<PhoneTypeViewModel, PhoneTypeView>()
				.RegisterWidgetForTabViewModel<EmailTypeViewModel, EmailTypeView>()
				.RegisterWidgetForTabViewModel<UserSettingsViewModel, UserSettingsView>()
				.RegisterWidgetForTabViewModel<PaymentLoaderViewModel, PaymentLoaderView>()
				.RegisterWidgetForTabViewModel<ManualPaymentMatchingViewModel, ManualPaymentMatchingView>()
				.RegisterWidgetForTabViewModel<ClientTaskViewModel, ClientTaskView>()
				.RegisterWidgetForTabViewModel<PaymentTaskViewModel, PaymentTaskView>()
				.RegisterWidgetForTabViewModel<DistrictsSetViewModel, DistrictsSetView>()
				.RegisterWidgetForTabViewModel<AcceptBeforeViewModel, AcceptBeforeView>()
				.RegisterWidgetForTabViewModel<OrderWithoutShipmentForDebtViewModel, OrderWithoutShipmentForDebtView>()
				.RegisterWidgetForTabViewModel<OrderWithoutShipmentForPaymentViewModel, OrderWithoutShipmentForPaymentView>()
				.RegisterWidgetForTabViewModel<OrderWithoutShipmentForAdvancePaymentViewModel, OrderWithoutShipmentForAdvancePaymentView>()
				.RegisterWidgetForTabViewModel<ReturnTareReasonCategoryViewModel, ReturnTareReasonCategoryView>()
				.RegisterWidgetForTabViewModel<ReturnTareReasonViewModel, ReturnTareReasonView>()
				.RegisterWidgetForTabViewModel<PaymentByCardViewModel, PaymentByCardView>()
				.RegisterWidgetForTabViewModel<RouteListAnalysisViewModel, RouteListAnalysisView>()
				.RegisterWidgetForTabViewModel<LateArrivalReasonViewModel, LateArrivalReasonView>()
				.RegisterWidgetForTabViewModel<NomenclatureViewModel, NomenclatureView>()
				.RegisterWidgetForTabViewModel<FinancialDistrictsSetViewModel, FinancialDistrictsSetView>()
				.RegisterWidgetForTabViewModel<MovementWagonViewModel, MovementWagonView>()
				.RegisterWidgetForTabViewModel<UserViewModel, UserView>()
                .RegisterWidgetForTabViewModel<ApplicationDevelopmentProposalViewModel, ApplicationDevelopmentProposalView>()
				.RegisterWidgetForTabViewModel<RegisteredRMViewModel, RegisteredRMView>()
				;

			//Регистрация виджетов
			ViewModelWidgetResolver.Instance
				.RegisterWidgetForWidgetViewModel<DistrictsSetJournalFilterViewModel, DistrictsSetJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DistrictJournalFilterViewModel, DistrictJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<SelectableParameterReportFilterViewModel, SelectableParameterReportFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintFilterViewModel, ComplaintFilterView>()
				.RegisterWidgetForWidgetViewModel<CounterpartyJournalFilterViewModel, CounterpartyFilterView>()
				.RegisterWidgetForWidgetViewModel<DebtorsJournalFilterViewModel, DebtorsFilterView>()
				.RegisterWidgetForWidgetViewModel<EmployeeFilterViewModel, EmployeeFilterView>()
				.RegisterWidgetForWidgetViewModel<OrderJournalFilterViewModel, OrderFilterView>()
				.RegisterWidgetForWidgetViewModel<ClientCameFromFilterViewModel, ClientCameFromFilterView>()
				.RegisterWidgetForWidgetViewModel<ResidueFilterViewModel, ResidueFilterView>()
				.RegisterWidgetForWidgetViewModel<IncomeCategoryJournalFilterViewModel, IncomeCategoryJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ExpenseCategoryJournalFilterViewModel, ExpenseCategoryJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<CashRequestJournalFilterViewModel, CashRequestJournalFilterView>() 
				.RegisterWidgetForWidgetViewModel<ProductGroupFilterViewModel, ProductGroupFilterView>()
				.RegisterWidgetForWidgetViewModel<FineFilterViewModel, FineFilterView>()
				.RegisterWidgetForWidgetViewModel<SubdivisionFilterViewModel, SubdivisionFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureFilterViewModel, NomenclaturesFilterView>()
				.RegisterWidgetForWidgetViewModel<RequestsToSuppliersFilterViewModel, RequestsToSuppliersFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureStockFilterViewModel, NomenclatureStockFilterView>()
				.RegisterWidgetForWidgetViewModel<OrderForMovDocJournalFilterViewModel, OrderForMovDocFilterView>()
				.RegisterWidgetForWidgetViewModel<BottlesCountAdvancedWageParameterViewModel, BottlesCountAdvancedWageParameterWidget>()
				.RegisterWidgetForWidgetViewModel<DeliveryTimeAdvancedWageParameterViewModel, DeliveryTimeAdvancedWagePrameterView>()
				.RegisterWidgetForWidgetViewModel<AdvancedWageParametersViewModel, AdvancedWageParametersView>()
				.RegisterWidgetForWidgetViewModel<AddFixPriceActionViewModel, AddFixPriceActionView>()
				.RegisterWidgetForWidgetViewModel<CarJournalFilterViewModel, CarFilterView>()
				.RegisterWidgetForWidgetViewModel<PresetSubdivisionPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<DeliveryPointJournalFilterViewModel, DeliveryPointJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<PaymentsJournalFilterViewModel, PaymentsFilterView>()
				.RegisterWidgetForWidgetViewModel<CallTaskFilterViewModel, CallTaskFilterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalFooterViewModel, BusinessTasksJournalFooterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalActionsViewModel, BusinessTasksJournalActionsView>()
				.RegisterWidgetForWidgetViewModel<SendDocumentByEmailViewModel, SendDocumentByEmailView>()
				.RegisterWidgetForWidgetViewModel<FinancialDistrictsSetsJournalFilterViewModel, FinancialDistrictsSetsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<FixedPricesViewModel, FixedPricesView>()
				.RegisterWidgetForWidgetViewModel<MovementWagonJournalFilterViewModel, MovementWagonJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<UserJournalFilterViewModel, UserJournalFilterView>()
                .RegisterWidgetForWidgetViewModel<ApplicationDevelopmentProposalsJournalFilterViewModel, ApplicationDevelopmentProposalsJournalFilterView>()
                .RegisterWidgetForWidgetViewModel<RouteListJournalFilterViewModel, RouteListJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<RegisteredRMJournalFilterViewModel, RegisteredRMJournalFilterView>()
				;

			DialogHelper.FilterWidgetResolver = ViewModelWidgetResolver.Instance;
		}

		static void ConfigureJournalColumnsConfigs()
		{
			JournalsColumnsConfigs.RegisterColumns();
		}

		static void CreateBaseConfig()
		{
			logger.Info("Настройка параметров базы...");
			//Увеличиваем таймоут
			QSMain.ConnectionString += ";ConnectionTimeout=120";

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(QSMain.ConnectionString)
				.AdoNetBatchSize(100)
				.Driver<LoggedMySqlClientDriver>();
			
			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config, 
				new System.Reflection.Assembly[] {
					System.Reflection.Assembly.GetAssembly (typeof(QS.Project.HibernateMapping.UserBaseMap)),
					System.Reflection.Assembly.GetAssembly (typeof(HibernateMapping.OrganizationMap)),
					System.Reflection.Assembly.GetAssembly (typeof(Bank)),
					System.Reflection.Assembly.GetAssembly (typeof(HistoryMain)),
				},
			(cnf) => {
					cnf.DataBaseIntegration(
						dbi => {
							dbi.BatchSize = 100;
							dbi.Batcher<MySqlClientBatchingBatcherFactory>();
						}
					);
				}
			);
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
				OrmObjectMapping<FuelType>.Create().Dialog<FuelTypeDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Стоимость", x => x.Cost.ToString()).End(),
				OrmObjectMapping<MovementWagon>.Create().Dialog<MovementWagonViewModel>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Остальные справочники
				OrmObjectMapping<CarProxyDocument>.Create().Dialog<CarProxyDlg>(),
				OrmObjectMapping<M2ProxyDocument>.Create().Dialog<M2ProxyDlg>(),
				OrmObjectMapping<ProductGroup>.Create().Dialog<ProductGroupDlg>(),
				OrmObjectMapping<CommentTemplate>.Create().Dialog<CommentTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Comment).End(),
				OrmObjectMapping<FineTemplate>.Create().Dialog<FineTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Reason).End(),
				OrmObjectMapping<PremiumTemplate>.Create().Dialog<PremiumTemplateDlg>().DefaultTableView().SearchColumn("Шаблон комментария", x => x.Reason).End(),
				OrmObjectMapping<MeasurementUnits>.Create ().Dialog<MeasurementUnitsDlg>().DefaultTableView().SearchColumn("ОКЕИ", x => x.OKEI).SearchColumn("Название", x => x.Name).Column("Точность", x => x.Digits.ToString()).End(),
				OrmObjectMapping<Contact>.Create().Dialog <ContactDlg>()
					.DefaultTableView().SearchColumn("Фамилия", x => x.Surname).SearchColumn("Имя", x => x.Name).SearchColumn("Отчество", x => x.Patronymic).End(),
				OrmObjectMapping<Order>.Create().Dialog <OrderDlg>().PopupMenu(OrderPopupMenu.GetPopupMenu),
				OrmObjectMapping<UndeliveredOrder>.Create().Dialog<UndeliveredOrderDlg>(),
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
				OrmObjectMapping<Counterparty>.Create().Dialog<CounterpartyDlg>().DefaultTableView().SearchColumn("Название", x => x.FullName).End(),
				OrmObjectMapping<Tag>.Create().Dialog<TagDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<CounterpartyContract>.Create().Dialog<CounterpartyContractDlg>(),
				OrmObjectMapping<DocTemplate>.Create().Dialog<DocTemplateDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Тип", x => x.TemplateType.GetEnumTitle()).End(),
				OrmObjectMapping<TransferOperationDocument>.Create().Dialog<TransferOperationDocumentDlg>(),
				//Справочники с фильтрами
				OrmObjectMapping<Equipment>.Create().Dialog<EquipmentDlg>().JournalFilter<EquipmentFilter>()
					.DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Номенклатура", x => x.NomenclatureName).Column("Тип", x => x.Nomenclature.Type.Name).SearchColumn("Серийный номер", x => x.Serial).Column("Дата последней обработки", x => x.LastServiceDate.ToShortDateString ()).End(),
				//Логисткика
				OrmObjectMapping<RouteList>.Create().Dialog<RouteListCreateDlg>()
					.DefaultTableView().SearchColumn("Номер", x => x.Id.ToString()).Column("Дата", x => x.Date.ToShortDateString()).Column("Статус", x => x.Status.GetEnumTitle ()).SearchColumn("Водитель", x => String.Format ("{0} - {1}", x.Driver.FullName, x.Car.Title)).End(),
				OrmObjectMapping<RouteColumn>.Create().DefaultTableView().Column("Код", x => x.Id.ToString()).SearchColumn("Название", x => x.Name).End(),
				OrmObjectMapping<DeliveryShift>.Create().Dialog<DeliveryShiftDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).SearchColumn("Диапазон времени", x => x.DeliveryTime).End(),
				OrmObjectMapping<DeliveryDaySchedule>.Create().Dialog<DeliveryDayScheduleDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).End(),
				//Сервис
				OrmObjectMapping<ServiceClaim>.Create().Dialog<ServiceClaimDlg>().DefaultTableView().Column("Номер", x => x.Id.ToString()).Column("Тип", x => x.ServiceClaimType.GetEnumTitle()).Column("Оборудование", x => x.Equipment.Title).Column("Подмена", x => x.ReplacementEquipment != null ? "Да" : "Нет").Column("Точка доставки", x => x.DeliveryPoint.Title).End(),
				//Касса
				OrmObjectMapping<Income>.Create ().Dialog<CashIncomeDlg> (),
				OrmObjectMapping<ExpenseCategory>.Create ().Dialog<ExpenseCategoryViewModel>().DefaultTableView ().Column("Код", x => x.Id.ToString()).SearchColumn ("Название", e => e.Name).Column ("Тип документа", e => e.ExpenseDocumentType.GetEnumTitle()).TreeConfig(new RecursiveTreeConfig<ExpenseCategory>(x => x.Parent, x => x.Childs)).End (),
				OrmObjectMapping<Expense>.Create ().Dialog<CashExpenseDlg> (),
				OrmObjectMapping<AdvanceReport>.Create ().Dialog<AdvanceReportDlg> (),
				OrmObjectMapping<Fine>.Create ().Dialog<FineDlg> (),
				OrmObjectMapping<Premium>.Create ().Dialog<PremiumDlg> (),
				OrmObjectMapping<IncomeCashTransferDocument>.Create().Dialog<IncomeCashTransferDlg>(),
				OrmObjectMapping<CommonCashTransferDocument>.Create().Dialog<CommonCashTransferDlg>(),
				//Банкинг
				OrmObjectMapping<AccountIncome>.Create (),
				OrmObjectMapping<AccountExpense>.Create (),
				//Склад
				OrmObjectMapping<Warehouse>.Create().Dialog<WarehouseDlg>().DefaultTableView().Column("Название", w=>w.Name).Column("В архиве", w=>w.IsArchive ? "Да":"").End(),
				OrmObjectMapping<RegradingOfGoodsTemplate>.Create().Dialog<RegradingOfGoodsTemplateDlg>().DefaultTableView().Column("Название", w=>w.Name).End()
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
			OrmMain.AddObjectDescription<Nomenclature>().Dialog<NomenclatureDlg>().JournalFilter<NomenclatureFilter>().
				DefaultTableView()
				.SearchColumn("Код", x => x.Id.ToString())
				.SearchColumn("Название", x => x.Name)
				.Column("Тип", x => x.CategoryString)
				.SearchColumn("Номер в ИМ", x => x.OnlineStoreExternalId)
				.End();
			OrmMain.AddObjectDescription<Folder1c>().Dialog<Folder1cDlg>()
				.DefaultTableView()
				.SearchColumn("Код 1С", x => x.Code1c)
				.SearchColumn("Название", x => x.Name)
				.TreeConfig(new RecursiveTreeConfig<Folder1c>(x => x.Parent, x => x.Childs)).End();
			#endregion

			#region Простые справочники
			OrmMain.AddObjectDescription<DiscountReason>()
				   .DefaultTableView()
				   .SearchColumn("Название", x => x.Name)
				   .End();
			OrmMain.AddObjectDescription<GeographicGroup>()
				   .Dialog<GeographicGroupDlg>()
				   .DefaultTableView()
				   .SearchColumn("Название", x => x.Name)
				   .Column("Код", x => x.Id.ToString())
				   .End();
			OrmMain.AddObjectDescription<TariffZone>()
				   .DefaultTableView()
				   .SearchColumn("Номер", x => x.Id.ToString())
				   .SearchColumn("Название", x => x.Name)
				   .End();
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

			#endregion

			#region неПростые справочники
			OrmMain.AddObjectDescription<Subdivision>().Dialog<SubdivisionDlg>().DefaultTableView().SearchColumn("Название", x => x.Name).Column("Руководитель", x => x.Chief == null ? "" : x.Chief.ShortName).SearchColumn("Номер", x => x.Id.ToString()).TreeConfig(new RecursiveTreeConfig<Subdivision>(x => x.ParentSubdivision, x => x.ChildSubdivisions)).End();
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
			OrmMain.AddObjectDescription<Employee>().Dialog<EmployeeDlg>().DefaultTableView()
				   .Column("Код", x => x.Id.ToString())
				   .SearchColumn("Ф.И.О.", x => x.FullName)
				   .Column("Категория", x => x.Category.GetEnumTitle())
				   .OrderAsc(x => x.LastName).OrderAsc(x => x.Name).OrderAsc(x => x.Patronymic)
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
				   .Column("Минимальная сумма заказа", x => x.OrderMinSumEShopGoods.ToString())
				   .SearchColumn("Описание правила", x => x.Title)
				   .End();
			OrmMain.AddObjectDescription<Car>().Dialog<CarsDlg>().DefaultTableView()
				   .SearchColumn("Код", x => x.Id.ToString())
				   .SearchColumn("Модель а/м", x => x.Model)
				   .SearchColumn("Гос. номер", x => x.RegistrationNumber)
				   .SearchColumn("Водитель", x => x.Driver != null ? x.Driver.FullName : string.Empty)
				   .End();
			OrmMain.AddObjectDescription<Certificate>().Dialog<CertificateDlg>().DefaultTableView()
				   .SearchColumn("Имя", x => x.Name)
				   .Column("Тип", x => x.TypeOfCertificate.GetEnumTitle())
				   .SearchColumn("Номер", x => x.Id.ToString())
				   .SearchColumn("Начало срока", x => x.StartDate.HasValue ? x.StartDate.Value.ToString("dd.MM.yyyy") : "Ошибка!")
				   .SearchColumn("Окончание срока", x => x.ExpirationDate.HasValue ? x.ExpirationDate.Value.ToString("dd.MM.yyyy") : "Бессрочно")
				   .Column("Архивный?", x => x.IsArchive ? "Да" : string.Empty)
				   .OrderAsc(x => x.IsArchive)
				   .OrderAsc(x => x.Id)
				   .End();
			OrmMain.AddObjectDescription<StoredImageResource>().Dialog<ImageLoaderDlg>().DefaultTableView()
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

			//Настройка ParentReference
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Organization, Account> {
				AddNewChild = (o, a) => o.AddAccount(a)
			});
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Counterparty, Account> {
				AddNewChild = (c, a) => c.AddAccount(a)
			});
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Employee, Account> {
				AddNewChild = (c, a) => c.AddAccount(a)
			});
			ParentReferenceConfig.AddActions(new ParentReferenceActions<Trainee, Account> {
				AddNewChild = (c, a) => c.AddAccount(a)
			});
		}
		
		static void GetPermissionsSettings() {
			string    sql = "SELECT * FROM permissions_settings";
			DbCommand cmd = QSMain.ConnectionDB.CreateCommand();
			cmd.CommandText = sql;
			using (DbDataReader rdr = cmd.ExecuteReader())
			{
				while (rdr.Read())
				{
					PermissionsSettings.PresetPermissions.Add(rdr["name"].ToString(),
						new PresetUserPermissionSource(rdr["name"].ToString(), rdr["display_name"].ToString(),
							string.IsNullOrEmpty(rdr["description"].ToString()) ? "" : rdr["description"].ToString()));
				}
			}
		}

		public static void CreateTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(!dirInfo.Exists) 
				dirInfo.Create();
		}

		public static void ClearTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(dirInfo.Exists) 
			{
				foreach(FileInfo file in dirInfo.EnumerateFiles()) {
					file.Delete();
				}
				foreach(DirectoryInfo dir in dirInfo.EnumerateDirectories()) {
					dir.Delete(true);
				}

				dirInfo.Delete();
			}
		}

		public static Autofac.IContainer AppDIContainer;

		static void AutofacClassConfig()
		{
			var builder = new ContainerBuilder();

			#region База
			builder.Register(c => UnitOfWorkFactory.GetDefaultFactory).As<IUnitOfWorkFactory>();
			builder.RegisterType<BaseParametersProvider>().AsSelf();
			#endregion

			#region Сервисы
			#region GtkUI
			builder.RegisterType<GtkMessageDialogsInteractive>().As<IInteractiveMessage>();
			builder.RegisterType<GtkQuestionDialogsInteractive>().As<IInteractiveQuestion>();
			builder.RegisterType<GtkInteractiveService>().As<IInteractiveService>();
			#endregion GtkUI
			builder.Register(c => ServicesConfig.CommonServices).As<ICommonServices>();
			builder.RegisterType<UserService>().As<IUserService>();
			#endregion

			#region Vodovoz
			#region Adapters
			builder.RegisterType<UndeliveriesViewOpener>().As<IUndeliveriesViewOpener>();
			#endregion
			#region Services
			builder.Register(c => VodovozGtkServicesConfig.EmployeeService).As<IEmployeeService>();
			builder.RegisterType<GtkFilePicker>().As<IFilePickerService>();
			builder.Register(c => new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), EmployeeSingletonRepository.GetInstance())).As<IEntityExtendedPermissionValidator>();
			builder.RegisterType<EmployeeService>().As<IEmployeeService>();
			#endregion
			#region Selectors
			builder.RegisterType<NomenclatureSelectorFactory>().As<INomenclatureSelectorFactory>();
			builder.RegisterType<OrderSelectorFactory>().As<IOrderSelectorFactory>();
			builder.RegisterType<RdlPreviewOpener>().As<IRDLPreviewOpener>();
			#endregion
			#region Интерфейсы репозиториев
			builder.RegisterType<SubdivisionRepository>().As<ISubdivisionRepository>();
			builder.Register(c => EmployeeSingletonRepository.GetInstance()).As<IEmployeeRepository>();
			builder.RegisterType<WarehouseRepository>().As<IWarehouseRepository>();
			builder.Register(c => UserSingletonRepository.GetInstance()).As<IUserRepository>();
			#endregion
			#region Mango
			builder.RegisterType<MangoManager>().AsSelf();
			#endregion
			#endregion

			#region Навигация
			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(AppDIContainer)).As<IViewModelsPageFactory>();
			builder.Register((ctx) => new AutofacTdiPageFactory(AppDIContainer)).As<ITdiPageFactory>();
			builder.Register((ctx) => new AutofacViewModelsGtkPageFactory(AppDIContainer)).AsSelf();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().As<ITdiCompatibilityNavigation>().SingleInstance();
			builder.Register(cc => new ClassNamesBaseGtkViewResolver(typeof(InternalTalkView), typeof(DeletionView))).As<IGtkViewResolver>();
			#endregion

			#region Старые диалоги
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(CounterpartyDlg)))
				.Where(t => t.IsAssignableTo<ITdiTab>())
				.AsSelf();
			#endregion

			#region Старые общие диалоги
			builder.RegisterType<ReportViewDlg>().AsSelf();
			#endregion

			#region ViewModels
			builder.Register(x => new AutofacViewModelResolver(AppDIContainer)).As<IViewModelResolver>();
			builder.RegisterAssemblyTypes(
					System.Reflection.Assembly.GetAssembly(typeof(InternalTalkViewModel)),
				 	System.Reflection.Assembly.GetAssembly(typeof(ComplaintViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			#endregion

			#region Repository
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(CounterpartyContractRepository)))
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf();
			#endregion

			AppDIContainer = builder.Build();
		}
	}
}
