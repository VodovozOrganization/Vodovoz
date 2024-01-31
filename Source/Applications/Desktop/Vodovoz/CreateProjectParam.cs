using Autofac;
using QS.Dialog.Gtk;
using QS.Permissions;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.Widgets.GtkUI;
using QSProjectsLib;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Vodovoz.AdministrationTools;
using Vodovoz.Cash;
using Vodovoz.Cash.DocumentsJournal;
using Vodovoz.Cash.FinancialCategoriesGroups;
using Vodovoz.Cash.Reports;
using Vodovoz.Cash.Transfer;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Counterparties;
using Vodovoz.Dialogs.Cash;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.Email;
using Vodovoz.Dialogs.Fuel;
using Vodovoz.Dialogs.Organizations;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.Filters.GtkViews;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Filters.Views;
using Vodovoz.FilterViewModels;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Footers.ViewModels;
using Vodovoz.Footers.Views;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.JournalFilters.Cash;
using Vodovoz.JournalFilters.Goods;
using Vodovoz.JournalFilters.Proposal;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewers.Complaints;
using Vodovoz.Logistic;
using Vodovoz.Presentation.ViewModels.PaymentType;
using Vodovoz.QualityControl.Reports;
using Vodovoz.Rent;
using Vodovoz.Reports;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Cash;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.Services.Permissions;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.AdministrationTools;
using Vodovoz.ViewModels.BaseParameters;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Cash.DocumentsJournal;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Cash.Reports;
using Vodovoz.ViewModels.Cash.Transfer;
using Vodovoz.ViewModels.Cash.Transfer.Journal;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Counterparties.ClientClassification;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Dialogs.Roboats;
using Vodovoz.ViewModels.Dialogs.Sales;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.GeoGroup;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Retail;
using Vodovoz.ViewModels.Journals.FilterViewModels.Roboats;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Logistic.DriversStopLists;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.QualityControl.Reports;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewModels.ReportsParameters.Orders;
using Vodovoz.ViewModels.ReportsParameters.Payments;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.Suppliers;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;
using Vodovoz.ViewModels.ViewModels.Complaints;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Flyers;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Payments;
using Vodovoz.ViewModels.ViewModels.Proposal;
using Vodovoz.ViewModels.ViewModels.Rent;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ViewModels.Retail;
using Vodovoz.ViewModels.ViewModels.Sale;
using Vodovoz.ViewModels.ViewModels.Security;
using Vodovoz.ViewModels.ViewModels.Settings;
using Vodovoz.ViewModels.ViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using Vodovoz.ViewModels.WageCalculation;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.Views;
using Vodovoz.Views.BaseParameters;
using Vodovoz.Views.BusinessTasks;
using Vodovoz.Views.Cash;
using Vodovoz.Views.Client;
using Vodovoz.Views.Client.CounterpartyClassification;
using Vodovoz.Views.Complaints;
using Vodovoz.Views.Contacts;
using Vodovoz.Views.Employees;
using Vodovoz.Views.Flyers;
using Vodovoz.Views.Goods;
using Vodovoz.Views.Logistic;
using Vodovoz.Views.Orders;
using Vodovoz.Views.Orders.OrdersWithoutShipment;
using Vodovoz.Views.Organization;
using Vodovoz.Views.Payments;
using Vodovoz.Views.Permissions;
using Vodovoz.Views.Print;
using Vodovoz.Views.Proposal;
using Vodovoz.Views.Rent;
using Vodovoz.Views.Reports;
using Vodovoz.Views.Retail;
using Vodovoz.Views.Roboats;
using Vodovoz.Views.Sale;
using Vodovoz.Views.Security;
using Vodovoz.Views.Settings;
using Vodovoz.Views.Store;
using Vodovoz.Views.Suppliers;
using Vodovoz.Views.Users;
using Vodovoz.Views.WageCalculation;
using Vodovoz.Views.Warehouse;
using Vodovoz.ViewWidgets;
using Vodovoz.ViewWidgets.AdvancedWageParameterViews;
using Vodovoz.ViewWidgets.Permissions;
using Vodovoz.ViewWidgets.PromoSetAction;
using ProductGroupView = Vodovoz.Views.Goods.ProductGroupView;
using UserView = Vodovoz.Views.Users.UserView;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics;
using Vodovoz.ViewModels.ReportsParameters.Production;
using Vodovoz.Views.ReportsParameters.Production;
using Vodovoz.ViewModels.ReportsParameters.Logistic;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ViewModels.ReportsParameters.Logistic.CarOwnershipReport;
using Vodovoz.ReportsParameters.Logistic;

namespace Vodovoz
{
	partial class Startup
	{
		internal static IDataBaseInfo DataBaseInfo;

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
					new PermissionMatrix<WarehousePermissionsType, Warehouse>(), "Доступ к складам", "warehouse_access")
			};

			WarehousePermissionService warehousePermissionService = new WarehousePermissionService
			{
				WarehousePermissionValidatorFactory = new WarehousePermissionValidatorFactory()
			};
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
				.RegisterWidgetForTabViewModel<OrganizationOwnershipTypeViewModel, OrganizationOwnershipTypeView>()
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
				.RegisterWidgetForTabViewModel<ComplaintResultOfCounterpartyViewModel, ComplaintResultOfCounterpartyView>()
				.RegisterWidgetForTabViewModel<ComplaintResultOfEmployeesViewModel, ComplaintResultOfEmployeesView>()
				.RegisterWidgetForTabViewModel<SubdivisionViewModel, SubdivisionView>()
				.RegisterWidgetForTabViewModel<FineViewModel, FineView>()
				.RegisterWidgetForTabViewModel<RequestToSupplierViewModel, RequestToSupplierView>()
				.RegisterWidgetForTabViewModel<WageDistrictViewModel, WageDistrictView>()
				.RegisterWidgetForTabViewModel<WageDistrictLevelRatesViewModel, WageDistrictLevelRatesView>()
				.RegisterWidgetForTabViewModel<EmployeeWageParameterViewModel, WageParameterView>()
				.RegisterWidgetForTabViewModel<SalesPlanViewModel, SalesPlanView>()
				.RegisterWidgetForTabViewModel<RouteListsOnDayViewModel, RouteListsOnDayView>()
				.RegisterWidgetForTabViewModel<RouteListCreateViewModel, RouteListCreateView>()
				.RegisterWidgetForTabViewModel<RouteListFastDeliveryMaxDistanceViewModel, RouteListFastDeliveryMaxDistanceView>()
				.RegisterWidgetForTabViewModel<RouteListMaxFastDeliveryOrdersViewModel, RouteListMaxFastDeliveryOrdersView>()
				.RegisterWidgetForTabViewModel<DriversStopListsViewModel, DriversStopListsView>()
				.RegisterWidgetForTabViewModel<FuelDocumentViewModel, FuelDocumentView>()
				.RegisterWidgetForTabViewModel<DriverWorkScheduleSetViewModel, DriverWorkScheduleSetView>()
				.RegisterWidgetForTabViewModel<DriverDistrictPrioritySetViewModel, DriverDistrictPrioritySetView>()
				.RegisterWidgetForTabViewModel<ComplaintKindViewModel, ComplaintKindView>()
				.RegisterWidgetForTabViewModel<ComplaintDetalizationViewModel, ComplaintDetalizationView>()
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
				.RegisterWidgetForTabViewModel<DriversInfoExportViewModel, DriversInfoExportView>()
				.RegisterWidgetForTabViewModel<LateArrivalReasonViewModel, LateArrivalReasonView>()
				.RegisterWidgetForTabViewModel<NomenclatureViewModel, NomenclatureView>()
				.RegisterWidgetForTabViewModel<FinancialDistrictsSetViewModel, FinancialDistrictsSetView>()
				.RegisterWidgetForTabViewModel<MovementWagonViewModel, MovementWagonView>()
				.RegisterWidgetForTabViewModel<UserViewModel, UserView>()
				.RegisterWidgetForTabViewModel<CarViewModel, CarView>()
				.RegisterWidgetForTabViewModel<ApplicationDevelopmentProposalViewModel, ApplicationDevelopmentProposalView>()
				.RegisterWidgetForTabViewModel<RegisteredRMViewModel, RegisteredRMView>()
				.RegisterWidgetForTabViewModel<SalesChannelViewModel, SalesChannelView>()
				.RegisterWidgetForTabViewModel<DeliveryPointResponsiblePersonTypeViewModel, DeliveryPointResponsiblePersonTypeView>()
				.RegisterWidgetForTabViewModel<NomenclaturePlanViewModel, NomenclaturePlanView>()
				.RegisterWidgetForTabViewModel<NomenclaturePlanReportViewModel, NomenclaturePlanReportView>()
				.RegisterWidgetForTabViewModel<OrganizationCashTransferDocumentViewModel, OrganizationCashTransferDocumentView>()
				.RegisterWidgetForTabViewModel<PremiumViewModel, PremiumView>()
				.RegisterWidgetForTabViewModel<PremiumRaskatGAZelleViewModel, PremiumRaskatGAZelleView>()
				.RegisterWidgetForTabViewModel<PremiumTemplateViewModel, PremiumTemplateView>()
				.RegisterWidgetForTabViewModel<CarEventTypeViewModel, CarEventTypeView>()
				.RegisterWidgetForTabViewModel<CarEventViewModel, CarEventView>()
				.RegisterWidgetForTabViewModel<DiscountReasonViewModel, DiscountReasonView>()
				.RegisterWidgetForTabViewModel<EmployeeViewModel, EmployeeView>()
				.RegisterWidgetForTabViewModel<DriverComplaintReasonViewModel, DriverComplaintReasonView>()
				.RegisterWidgetForTabViewModel<FlyerViewModel, FlyerView>()
				.RegisterWidgetForTabViewModel<ComplaintObjectViewModel, ComplaintObjectView>()
				.RegisterWidgetForTabViewModel<DriverAttachedTerminalViewModel, DriverAttachedTerminalView>()
				.RegisterWidgetForTabViewModel<DeliveryPointViewModel, DeliveryPointView>()
				.RegisterWidgetForTabViewModel<DocumentsPrinterViewModel, DocumentsPrinterView>()
				.RegisterWidgetForTabViewModel<PrintOrdersDocumentsViewModel, PrintOrdersDocumentsView>()
				.RegisterWidgetForTabViewModel<CarModelViewModel, CarModelView>()
				.RegisterWidgetForTabViewModel<EquipmentKindViewModel, EquipmentKindView>()
				.RegisterWidgetForTabViewModel<ProductGroupViewModel, ProductGroupView>()
				.RegisterWidgetForTabViewModel<CarManufacturerViewModel, CarManufacturerView>()
				.RegisterWidgetForTabViewModel<UndeliveryTransferAbsenceReasonViewModel, UndeliveryTransferAbsenceReasonView>()
				.RegisterWidgetForTabViewModel<CashlessRequestViewModel, CashlessRequestView>()
				.RegisterWidgetForTabViewModel<CreateManualPaymentFromBankClientViewModel, CreateManualPaymentFromBankClientView>()
				.RegisterWidgetForTabViewModel<FreeRentPackageViewModel, FreeRentPackageView>()
				.RegisterWidgetForTabViewModel<PaidRentPackageViewModel, PaidRentPackageView>()
				.RegisterWidgetForTabViewModel<GeneralSettingsViewModel, GeneralSettingsView>()
				.RegisterWidgetForTabViewModel<RoboAtsCounterpartyNameViewModel, RoboAtsCounterpartyNameView>()
				.RegisterWidgetForTabViewModel<RoboAtsCounterpartyPatronymicViewModel, RoboAtsCounterpartyPatronymicView>()
				.RegisterWidgetForTabViewModel<TariffZoneViewModel, TariffZoneView>()
				.RegisterWidgetForTabViewModel<DeliveryScheduleViewModel, DeliveryScheduleView>()
				.RegisterWidgetForTabViewModel<RoboatsCatalogExportViewModel, RoboatsCatalogExportView>()
				.RegisterWidgetForTabViewModel<ComplaintsJournalsViewModel, ComplaintsJournalsView>()
				.RegisterWidgetForTabViewModel<RoboatsWaterTypeViewModel, RoboatsWaterTypeView>()
				.RegisterWidgetForTabViewModel<RoboatsStreetViewModel, RoboatsStreetView>()
				.RegisterWidgetForTabViewModel<FastDeliveryAvailabilityHistoryViewModel, FastDeliveryAvailabilityHistoryView>()
				.RegisterWidgetForTabViewModel<BulkEmailEventReasonViewModel, BulkEmailEventReasonView>()
				.RegisterWidgetForTabViewModel<GeoGroupViewModel, GeoGroupView>()
				.RegisterWidgetForTabViewModel<NomenclatureGroupPricingViewModel, NomenclatureGroupPricingView>()
				.RegisterWidgetForTabViewModel<RouteListMileageCheckViewModel, Vodovoz.Views.Logistic.RouteListMileageCheckView>()
				.RegisterWidgetForTabViewModel<RouteListTransferringViewModel, RouteListAddressesTransferringView>()
				.RegisterWidgetForTabViewModel<RouteListMileageDistributionViewModel, RouteListMileageDistributionView>()
				.RegisterWidgetForTabViewModel<FastDeliveryVerificationDetailsViewModel, FastDeliveryVerificationDetailsView>()
				.RegisterWidgetForTabViewModel<FastDeliveryOrderTransferViewModel, FastDeliveryOrderTransferView>()
				.RegisterWidgetForTabViewModel<RdlViewerViewModel, RdlViewerView>()
				.RegisterWidgetForTabViewModel<ResponsibleViewModel, ResponsibleView>()
				.RegisterWidgetForTabViewModel<EdoOperatorViewModel, EdoOperatorView>()
				.RegisterWidgetForTabViewModel<CounterpartyDetailsFromRevenueServiceViewModel, CounterpartyDetailsFromRevenueServiceView>()
				.RegisterWidgetForTabViewModel<CloseSupplyToCounterpartyViewModel, CloseSupplyToCounterpartyView>()
				.RegisterWidgetForTabViewModel<ResendCounterpartyEdoDocumentsViewModel, ResendCounterpartyEdoDocumentsView>()
				.RegisterWidgetForTabViewModel<DeliveryPriceRuleViewModel, DeliveryPriceRuleView>()
				.RegisterWidgetForTabViewModel<ExternalCounterpartyMatchingViewModel, ExternalCounterpartyMatchingView>()
				.RegisterWidgetForTabViewModel<InventoryInstanceViewModel, InventoryInstanceView>()
				.RegisterWidgetForTabViewModel<WriteOffDocumentViewModel, WriteoffDocumentView>()
				.RegisterWidgetForTabViewModel<InventoryDocumentViewModel, InventoryDocumentView>()
				.RegisterWidgetForTabViewModel<ShiftChangeResidueDocumentViewModel, ShiftChangeResidueDocumentView>()
				.RegisterWidgetForTabViewModel<InventoryInstanceMovementReportViewModel, InventoryInstanceMovementReportView>()
				.RegisterWidgetForTabViewModel<UndeliveryObjectViewModel, UndeliveryObjectView>()
				.RegisterWidgetForTabViewModel<UndeliveryKindViewModel, UndeliveryKindView>()
				.RegisterWidgetForTabViewModel<UndeliveryDetalizationViewModel, UndeliveryDetalizationView>()
				.RegisterWidgetForTabViewModel<RegradingOfGoodsReasonViewModel, RegradingOfGoodsReasonView>()
				.RegisterWidgetForTabViewModel<CounterpartyClassificationCalculationViewModel, CounterpartyClassificationCalculationView>()
				.RegisterWidgetForTabViewModel<NomenclatureOnlineGroupViewModel, NomenclatureOnlineGroupView>()
				.RegisterWidgetForTabViewModel<NomenclatureOnlineCategoryViewModel, NomenclatureOnlineCategoryView>()
				.RegisterWidgetForTabViewModel<DriverWarehouseEventViewModel, DriverWarehouseEventView>()
				.RegisterWidgetForTabViewModel<DriversWarehousesEventsReportViewModel, DriversWarehousesEventsReportView>()
				.RegisterWidgetForTabViewModel<RouteColumnViewModel, RouteColumnView>()
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
				.RegisterWidgetForWidgetViewModel<DriverMessageFilterViewModel, DriverMessageFilterView>()
				.RegisterWidgetForWidgetViewModel<ClientCameFromFilterViewModel, ClientCameFromFilterView>()
				.RegisterWidgetForWidgetViewModel<OrganizationOwnershipTypeJournalFilterViewModel, OrganizationOwnershipTypeJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ResidueFilterViewModel, ResidueFilterView>()
				.RegisterWidgetForWidgetViewModel<IncomeCategoryJournalFilterViewModel, IncomeCategoryJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ExpenseCategoryJournalFilterViewModel, ExpenseCategoryJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<PayoutRequestJournalFilterViewModel, PayoutRequestJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ProductGroupFilterViewModel, ProductGroupFilterView>()
				.RegisterWidgetForWidgetViewModel<FineFilterViewModel, FineFilterView>()
				.RegisterWidgetForWidgetViewModel<SubdivisionFilterViewModel, SubdivisionFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureFilterViewModel, NomenclaturesFilterView>()
				.RegisterWidgetForWidgetViewModel<RequestsToSuppliersFilterViewModel, RequestsToSuppliersFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureStockFilterViewModel, NomenclatureStockFilterView>()
				.RegisterWidgetForWidgetViewModel<OrderForMovDocJournalFilterViewModel, OrderForMovDocFilterView>()
				.RegisterWidgetForWidgetViewModel<CarModelJournalFilterViewModel, CarModelFilterView>()
				.RegisterWidgetForWidgetViewModel<BottlesCountAdvancedWageParameterViewModel, BottlesCountAdvancedWageParameterWidget>()
				.RegisterWidgetForWidgetViewModel<DeliveryTimeAdvancedWageParameterViewModel, DeliveryTimeAdvancedWagePrameterView>()
				.RegisterWidgetForWidgetViewModel<AdvancedWageParametersViewModel, AdvancedWageParametersView>()
				.RegisterWidgetForWidgetViewModel<AddFixPriceActionViewModel, AddFixPriceActionView>()
				.RegisterWidgetForWidgetViewModel<CarJournalFilterViewModel, CarFilterView>()
				.RegisterWidgetForWidgetViewModel<PresetUserPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<PresetSubdivisionPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<DeliveryPointJournalFilterViewModel, DeliveryPointJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<PaymentsJournalFilterViewModel, PaymentsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<CallTaskFilterViewModel, CallTaskFilterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalFooterViewModel, BusinessTasksJournalFooterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalActionsViewModel, BusinessTasksJournalActionsView>()
				.RegisterWidgetForWidgetViewModel<SendDocumentByEmailViewModel, SendDocumentByEmailView>()
				.RegisterWidgetForWidgetViewModel<FinancialDistrictsSetsJournalFilterViewModel, FinancialDistrictsSetsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<FixedPricesViewModel, FixedPricesView>()
				.RegisterWidgetForWidgetViewModel<MovementWagonJournalFilterViewModel, MovementWagonJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ApplicationDevelopmentProposalsJournalFilterViewModel, ApplicationDevelopmentProposalsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<RouteListJournalFilterViewModel, RouteListJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<RegisteredRMJournalFilterViewModel, RegisteredRMJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DeliveryPointResponsiblePersonTypeJournalFilterViewModel, DeliveryPointResponsiblePersonTypeJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<SalesChannelJournalFilterViewModel, SalesChannelJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<EmployeePostViewModel, EmployeePostView>()
				.RegisterWidgetForWidgetViewModel<WarehouseViewModel, WarehouseView>()
				.RegisterWidgetForWidgetViewModel<OrderAnalyticsReportViewModel, OrderAnalyticsReportView>()
				.RegisterWidgetForWidgetViewModel<CarsExploitationReportViewModel, CarsExploitationReportView>()
				.RegisterWidgetForWidgetViewModel<NomenclaturePlanFilterViewModel, NomenclaturePlanFilterView>()
				.RegisterWidgetForWidgetViewModel<OrganizationCashTransferDocumentFilterViewModel, OrganizationCashTransferDocumentFilterView>()
				.RegisterWidgetForWidgetViewModel<PremiumJournalFilterViewModel, PremiumJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DeliveryAnalyticsViewModel, DeliveryAnalyticsReportView>()
				.RegisterWidgetForWidgetViewModel<AdditionalLoadingSettingsViewModel, AdditionalLoadingSettingsView>()
				.RegisterWidgetForWidgetViewModel<CarEventFilterViewModel, CarEventFilterView>()
				.RegisterWidgetForWidgetViewModel<UndeliveredOrdersFilterViewModel, UndeliveredOrdersFilterView>()
				.RegisterWidgetForWidgetViewModel<DriverComplaintReasonJournalFilterViewModel, DriverComplaintReasonJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintObjectJournalFilterViewModel, ComplaintObjectJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintKindJournalFilterViewModel, ComplaintKindJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintDetalizationJournalFilterViewModel, ComplaintDetalizationJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<GeoGroupJournalFilterViewModel, GeoGroupJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<WarehousesBalanceSummaryViewModel, WarehousesBalanceSummaryView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureBalanceByStockFilterViewModel, NomenclatureBalanceByStockFilterView>()
				.RegisterWidgetForWidgetViewModel<SalaryByEmployeeJournalFilterViewModel, SalaryByEmployeeFilterView>()
				.RegisterWidgetForWidgetViewModel<ProductGroupJournalFilterViewModel, ProductGroupJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<UndeliveryTransferAbsenceReasonJournalFilterViewModel, UndeliveryTransferAbsenceReasonJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ProductionWarehouseMovementReportViewModel, ProductionWarehouseMovementReportView>()
				.RegisterWidgetForWidgetViewModel<WarehousePermissionsViewModel, WarehousePermissionView>()
				.RegisterWidgetForWidgetViewModel<TrackPointJournalFilterViewModel, TrackPointJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<FastDeliveryAvailabilityFilterViewModel, FastDeliveryAvailabilityFilterView>()
				.RegisterWidgetForWidgetViewModel<CostCarExploitationReportViewModel, CostCarExploitationReportView>()
				.RegisterWidgetForWidgetViewModel<FastDeliverySalesReportViewModel, FastDeliverySalesReportView>()
				.RegisterWidgetForWidgetViewModel<FastDeliveryAdditionalLoadingReportViewModel, FastDeliveryAdditionalLoadingReportView>()
				.RegisterWidgetForWidgetViewModel<RoboatsCallsFilterViewModel, RoboatsCallsFilterView>()
				.RegisterWidgetForWidgetViewModel<DeliveryScheduleFilterViewModel, DeliveryScheduleFilterView>()
				.RegisterWidgetForWidgetViewModel<BulkEmailEventReportViewModel, BulkEmailEventReportView>()
				.RegisterWidgetForWidgetViewModel<EdoUpdReportViewModel, EdoUpdReportView>()
				.RegisterWidgetForWidgetViewModel<ProfitabilitySalesReportViewModel, ProfitabilitySalesReportView>()
				.RegisterWidgetForWidgetViewModel<PhonesJournalFilterViewModel, PhonesJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<UsersJournalFilterViewModel, UsersJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<CarsMonitoringViewModel, CarsMonitoringView>()
				.RegisterWidgetForWidgetViewModel<TurnoverWithDynamicsReportViewModel, TurnoverWithDynamicsReportView>()
				.RegisterWidgetForWidgetViewModel<SalesBySubdivisionsAnalitycsReportViewModel, SalesBySubdivisionsAnalitycsReportView>()
				.RegisterWidgetForWidgetViewModel<FastDeliveryPercentCoverageReportViewModel, FastDeliveryPercentCoverageReportView>()
				.RegisterWidgetForWidgetViewModel<CashReceiptJournalFilterViewModel, TrueMarkReceiptJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<WarehouseDocumentsItemsJournalFilterViewModel, WarehouseDocumentsItemsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ExternalCounterpartiesMatchingJournalFilterViewModel, ExternalCounterpartiesMatchingJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<FinancialIncomeCategoryViewModel, FinancialIncomeCategoryView>()
				.RegisterWidgetForWidgetViewModel<FinancialExpenseCategoryViewModel, FinancialExpenseCategoryView>()
				.RegisterWidgetForWidgetViewModel<InventoryInstancesJournalFilterViewModel, InventoryInstancesJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<InventoryInstancesStockBalanceJournalFilterViewModel, InventoryInstancesStockBalanceJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<FinancialCategoriesGroupViewModel, FinancialCategoriesGroupView>()
				.RegisterWidgetForWidgetViewModel<FinancialCategoriesJournalFilterViewModel, FinancialCategoriesJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DocumentsFilterViewModel, DocumentsFilterView>()
				.RegisterWidgetForWidgetViewModel<ExpenseViewModel, ExpenseView>()
				.RegisterWidgetForWidgetViewModel<IncomeViewModel, IncomeView>()
				.RegisterWidgetForWidgetViewModel<ExpenseSelfDeliveryViewModel, ExpenseSelfDeliveryView>()
				.RegisterWidgetForWidgetViewModel<IncomeSelfDeliveryViewModel, IncomeSelfDeliveryView>()
				.RegisterWidgetForWidgetViewModel<AdvanceReportViewModel, AdvanceReportView>()
				.RegisterWidgetForWidgetViewModel<IncomeCashTransferDocumentViewModel, IncomeCashTransferView>()
				.RegisterWidgetForWidgetViewModel<CommonCashTransferDocumentViewModel, CommonCashTransferView>()
				.RegisterWidgetForWidgetViewModel<TransferDocumentsJournalFilterViewModel, CashTransferDocumentsFilter>()
				.RegisterWidgetForWidgetViewModel<CargoDailyNormViewModel, CargoDailyNormView>()
				.RegisterWidgetForWidgetViewModel<TransferExpenseViewModel, TransferExpenseView>()
				.RegisterWidgetForWidgetViewModel<TransferIncomeViewModel, TransferIncomeView>()
				.RegisterWidgetForWidgetViewModel<SearchViewModel, SearchView>()
				.RegisterWidgetForWidgetViewModel<CashFlowAnalysisViewModel, CashFlowAnalysisView>()
				.RegisterWidgetForWidgetViewModel<UndeliveryObjectJournalFilterViewModel, UndeliveryObjectFilterView>()
				.RegisterWidgetForWidgetViewModel<UndeliveryKindJournalFilterViewModel, UndeliveryKindFilterView>()
				.RegisterWidgetForWidgetViewModel<UndeliveryDetalizationJournalFilterViewModel, UndeliveryDetalizationFilterView>()
				.RegisterWidgetForWidgetViewModel<UndeliveredOrderViewModel, UndeliveredOrderView>()
				.RegisterWidgetForWidgetViewModel<DriverStopListRemovalViewModel, DriverStopListRemovalView>()
				.RegisterWidgetForWidgetViewModel<SubtypeViewModel, SubtypeView>()
				.RegisterWidgetForWidgetViewModel<BaseParametersViewModel, BaseParametersView>()
				.RegisterWidgetForWidgetViewModel<UndeliveredOrdersClassificationReportViewModel, UndeliveredOrdersClassificationReportView>()
				.RegisterWidgetForWidgetViewModel<NumberOfComplaintsAgainstDriversReportViewModel, NumberOfComplaintsAgainstDriversReportView>()
				.RegisterWidgetForWidgetViewModel<MovementsPaymentControlViewModel, MovementsPaymentControlView>()
				.RegisterWidgetForWidgetViewModel<SalesReportViewModel, SalesReportView>()
				.RegisterWidgetForWidgetViewModel<SetBillsReportViewModel, SetBillsReportView>()
				.RegisterWidgetForWidgetViewModel<RevisionReportViewModel, Revision>()
				.RegisterWidgetForWidgetViewModel<PaymentsFromBankClientReportViewModel, PaymentsFromBankClientReportView>()
				.RegisterWidgetForWidgetViewModel<RevenueServiceMassCounterpartyUpdateToolViewModel, RevenueServiceMassCounterpartyUpdateToolView>()
				.RegisterWidgetForWidgetViewModel<FastDeliveryOrderTransferFilterViewModel, FastDeliveryOrderTransferFilterView>()
				.RegisterWidgetForWidgetViewModel<WarehousesSettingsViewModel, NamedDomainEntitiesSettingsView>()
				.RegisterWidgetForWidgetViewModel<SelectPaymentTypeViewModel, SelectPaymentTypeWindowView>()
				.RegisterWidgetForWidgetViewModel<FreeRentPackagesFilterViewModel, FreeRentPackagesFilterView>()
				.RegisterWidgetForWidgetViewModel<CounterpartyClassificationCalculationEmailSettingsViewModel, CounterpartyClassificationCalculationEmailSettingsView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureOnlineCatalogViewModel, NomenclatureOnlineCatalogView>()
				.RegisterWidgetForWidgetViewModel<DriversWarehousesEventsJournalFilterViewModel, DriversWarehousesEventsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<CompletedDriversWarehousesEventsJournalFilterViewModel, CompletedDriversWarehousesEventsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ProducedProductionReportViewModel, ProducedProductionReportView>()
				.RegisterWidgetForWidgetViewModel<DeliveryTimeReportViewModel, DeliveryTimeReportView>()
				.RegisterWidgetForWidgetViewModel<CarOwnershipReportViewModel, CarOwnershipReportView>()
				;

			DialogHelper.FilterWidgetResolver = ViewModelWidgetResolver.Instance;
		}

		static void ConfigureJournalColumnsConfigs()
		{
			JournalsColumnsConfigs.RegisterColumns();
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

		public static ILifetimeScope AppDIContainer;
	}
}
