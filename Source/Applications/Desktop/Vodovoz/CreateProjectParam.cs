using Autofac;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.Deletion.ViewModels;
using QS.Deletion.Views;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Project.Services.GtkUI;
using QS.Report;
using QS.Report.Repository;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Extension;
using QS.ViewModels.Resolve;
using QS.Views.Resolve;
using QS.Widgets.GtkUI;
using QSProjectsLib;
using QSReport;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Core.DataService;
using Vodovoz.Core.Permissions;
using Vodovoz.Dialogs.Cash;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.Email;
using Vodovoz.Dialogs.Fuel;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
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
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.JournalFilters.Proposal;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewers.Complaints;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bookkeeping;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.Services;
using Vodovoz.Services.Permissions;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Dialogs.Roboats;
using Vodovoz.ViewModels.Dialogs.Sales;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Retail;
using Vodovoz.ViewModels.Journals.FilterViewModels.Roboats;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Mango.Talks;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.Reports;
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
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Retail;
using Vodovoz.ViewModels.ViewModels.Security;
using Vodovoz.ViewModels.ViewModels.Settings;
using Vodovoz.ViewModels.ViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Vodovoz.ViewModels.WageCalculation;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Views;
using Vodovoz.Views.BusinessTasks;
using Vodovoz.Views.Cash;
using Vodovoz.Views.Client;
using Vodovoz.Views.Complaints;
using Vodovoz.Views.Contacts;
using Vodovoz.Views.Employees;
using Vodovoz.Views.Flyers;
using Vodovoz.Views.Goods;
using Vodovoz.Views.Logistic;
using Vodovoz.Views.Mango.Talks;
using Vodovoz.Views.Orders;
using Vodovoz.Views.Orders.OrdersWithoutShipment;
using Vodovoz.Views.Organization;
using Vodovoz.Views.Payments;
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
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using Vodovoz.Views.Permissions;
using UserView = Vodovoz.Views.Users.UserView;
using ProductGroupView = Vodovoz.Views.Goods.ProductGroupView;
using Vodovoz.ReportsParameters.Orders;
using QS.DomainModel.NotifyChange;
using QS.Project.Domain;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using Vodovoz.ViewModels.Dialogs.Sales;
using Vodovoz.Views.Sale;
using Vodovoz.Models;
using QS.Validation;
using Vodovoz.ViewWidgets;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.Reports;
using Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;
using Vodovoz.Domain.Permissions;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;

namespace Vodovoz
{
	partial class MainClass
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
				.RegisterWidgetForTabViewModel<FuelDocumentViewModel, FuelDocumentView>()
				.RegisterWidgetForTabViewModel<DriverWorkScheduleSetViewModel, DriverWorkScheduleSetView>()
				.RegisterWidgetForTabViewModel<DriverDistrictPrioritySetViewModel, DriverDistrictPrioritySetView>()
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
				.RegisterWidgetForTabViewModel<RoboatsWaterTypeViewModel, RoboatsWaterTypeView>()
				.RegisterWidgetForTabViewModel<RoboatsStreetViewModel, RoboatsStreetView>()
				.RegisterWidgetForTabViewModel<FastDeliveryAvailabilityHistoryViewModel, FastDeliveryAvailabilityHistoryView>()
				.RegisterWidgetForTabViewModel<BulkEmailEventReasonViewModel, BulkEmailEventReasonView>()
				.RegisterWidgetForTabViewModel<GeoGroupViewModel, GeoGroupView>()
				.RegisterWidgetForTabViewModel<NomenclatureGroupPricingViewModel, NomenclatureGroupPricingView>()
				.RegisterWidgetForTabViewModel<RouteListMileageCheckViewModel, Vodovoz.Views.Logistic.RouteListMileageCheckView>()
				.RegisterWidgetForTabViewModel<RouteListMileageDistributionViewModel, RouteListMileageDistributionView>()
				.RegisterWidgetForTabViewModel<FastDeliveryVerificationDetailsViewModel, FastDeliveryVerificationDetailsView>()
				.RegisterWidgetForTabViewModel<RdlViewerViewModel, RdlViewerView>()
				.RegisterWidgetForTabViewModel<ResponsibleViewModel, ResponsibleView>()
				.RegisterWidgetForTabViewModel<EdoOperatorViewModel, EdoOperatorView>()
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

		public static Autofac.IContainer AppDIContainer;

		static void AutofacClassConfig()
		{
			var builder = new ContainerBuilder();

			#region База

			builder.Register(c => UnitOfWorkFactory.GetDefaultFactory).As<IUnitOfWorkFactory>();
			builder.RegisterInstance(DataBaseInfo).As<IDataBaseInfo>();

			#endregion

			#region Репозитории

			builder.RegisterType<UserPrintingRepository>().As<IUserPrintingRepository>().SingleInstance();

			#endregion

			#region Сервисы

			//GtkUI
			builder.RegisterType<GtkMessageDialogsInteractive>().As<IInteractiveMessage>();
			builder.RegisterType<GtkQuestionDialogsInteractive>().As<IInteractiveQuestion>();
			builder.RegisterType<GtkInteractiveService>().As<IInteractiveService>();

			builder.Register(c => ServicesConfig.CommonServices).As<ICommonServices>();
			builder.RegisterType<UserService>().As<IUserService>();
			builder.RegisterType<DeleteEntityGUIService>().As<IDeleteEntityService>();
			builder.Register(c => DeleteConfig.Main).As<DeleteConfiguration>();
			builder.Register(c => PermissionsSettings.CurrentPermissionService).As<ICurrentPermissionService>();
			builder.RegisterType<ReportPrinter>().As<IReportPrinter>();

			builder.RegisterType<EntityDeleteWorker>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<CommonMessages>().AsSelf();

			#endregion

			#region Старые общие диалоги

			builder.RegisterType<ReportViewDlg>().AsSelf();

			#endregion

			#region Навигация

			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(AppDIContainer)).As<IViewModelsPageFactory>();
			builder.Register((ctx) => new AutofacTdiPageFactory(AppDIContainer)).As<ITdiPageFactory>();
			builder.Register((ctx) => new AutofacViewModelsGtkPageFactory(AppDIContainer)).AsSelf();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().As<ITdiCompatibilityNavigation>()
				.SingleInstance();
			builder.Register(cc => new ClassNamesBaseGtkViewResolver(
				typeof(InternalTalkView),
				typeof(DeletionView),
				typeof(RdlViewerView))
			).As<IGtkViewResolver>();

			#endregion

			#region ViewModels

			builder.Register(x => new AutofacViewModelResolver(AppDIContainer)).As<IViewModelResolver>();
			builder.Register(с => NotifyConfiguration.Instance).As<IEntityChangeWatcher>();
			builder.RegisterAssemblyTypes(
					Assembly.GetAssembly(typeof(InternalTalkViewModel)),
					Assembly.GetAssembly(typeof(ComplaintViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			builder.RegisterType<PrepareDeletionViewModel>().As<IOnCloseActionViewModel>().AsSelf();
			builder.RegisterType<DeletionProcessViewModel>().As<IOnCloseActionViewModel>().AsSelf();
			builder.RegisterType<DeletionViewModel>().AsSelf();
			builder.RegisterType<RdlViewerViewModel>().AsSelf();
			builder.RegisterType<ProgressWindowViewModel>().AsSelf();

			#endregion

			RegisterVodovozClassConfig(builder);

			AppDIContainer = builder.Build();
		}

		static void RegisterVodovozClassConfig(ContainerBuilder builder)
		{
			builder.RegisterType<WaterFixedPricesGenerator>().AsSelf();
			builder.RegisterInstance(ViewModelWidgetResolver.Instance).AsSelf().AsImplementedInterfaces();

			#region Adapters & Factories

			builder.RegisterType<GtkTabsOpener>().As<IGtkTabsOpener>();
			builder.RegisterType<UndeliveredOrdersJournalOpener>().As<IUndeliveredOrdersJournalOpener>();
			builder.RegisterType<RdlPreviewOpener>().As<IRDLPreviewOpener>();
			builder.RegisterType<GtkReportViewOpener>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(
					Assembly.GetExecutingAssembly(),
					Assembly.GetAssembly(typeof(VodovozBusinessAssemblyFinder)),
					Assembly.GetAssembly(typeof(VodovozViewModelAssemblyFinder)))
				.Where(t => t.Name.EndsWith("Factory"))
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance();

			#endregion

			#region Controllers

			builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(VodovozBusinessAssemblyFinder)))
				.Where(t => t.Name.EndsWith("Controller") || t.Name.EndsWith("Handler"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterType<GeoGroupVersionsModel>().SingleInstance().AsSelf().AsImplementedInterfaces();

			#endregion

			#region Services

			builder.Register(c => VodovozGtkServicesConfig.EmployeeService).As<IEmployeeService>();
			builder.RegisterType<FileDialogService>().As<IFileDialogService>();
			builder.Register(c => PermissionExtensionSingletonStore.GetInstance()).As<IPermissionExtensionStore>();
			builder.RegisterType<EntityExtendedPermissionValidator>().As<IEntityExtendedPermissionValidator>();
			builder.RegisterType<EmployeeService>().As<IEmployeeService>();
			builder.Register(c => PermissionsSettings.PermissionService).As<IPermissionService>();
			builder.Register(c => ErrorReporter.Instance).As<IErrorReporter>();
			builder.RegisterType<ObjectValidator>().AsImplementedInterfaces().AsSelf();
			builder.RegisterType<WarehousePermissionService>().AsImplementedInterfaces().AsSelf();

			#endregion

			#region Models

			builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(VodovozBusinessAssemblyFinder)))
				.Where(t => t.Name.EndsWith("Model") && !t.Name.EndsWith("ViewModel"))
				.AsImplementedInterfaces()
				.AsSelf();
			builder.RegisterType<WageParameterService>().AsSelf();
			builder.RegisterType<UserWarehousePermissionModel>()
				.As<WarehousePermissionModelBase>()
				.AsSelf();

			#endregion

			#region CallTasks

			builder.Register(context => CallTaskSingletonFactory.GetInstance()).As<ICallTaskFactory>();

			builder.Register(context => new CallTaskWorker(
				context.Resolve<ICallTaskFactory>(),
				context.Resolve<ICallTaskRepository>(),
				context.Resolve<IOrderRepository>(),
				context.Resolve<IEmployeeRepository>(),
				context.Resolve<IPersonProvider>(),
				context.Resolve<IUserService>(),
				context.Resolve<IErrorReporter>()
			)).As<ICallTaskWorker>();

			#endregion

			#region Репозитории

			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(CounterpartyContractRepository)))
				.Where(t => t.Name.EndsWith("Repository"))
				.AsImplementedInterfaces()
				.SingleInstance();

			#endregion

			#region Mango

			builder.RegisterType<MangoManager>().AsSelf();

			#endregion

			#region Reports

			builder.RegisterType<CounterpartyCashlessDebtsReport>().AsSelf();
			builder.RegisterType<OrderChangesReport>().AsSelf();

			#endregion

			#region Старые диалоги

			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(CounterpartyDlg)))
				.Where(t => t.IsAssignableTo<ITdiTab>())
				.AsSelf();

			#endregion

			#region ParameterProviders

			builder.RegisterType<BaseParametersProvider>()
				.As<ITerminalNomenclatureProvider>()
				.As<IProfitCategoryProvider>()
				.AsSelf();

			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(ParametersProvider)))
				.Where(t => t.Name.EndsWith("Provider"))
				.AsImplementedInterfaces()
				.AsSelf()
				.SingleInstance();

			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(ParametersProvider)))
				.Where(t => t.Name.EndsWith("Settings"))
				.AsImplementedInterfaces()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<RdlViewerViewModel>().AsSelf();

			#endregion

			#region Фильтры

			builder.RegisterType<PaymentsJournalFilterViewModel>().AsSelf();
			builder.RegisterType<UnallocatedBalancesJournalFilterViewModel>().AsSelf();

			#endregion

			#region Классы

			builder.RegisterType<Domain.Employees.User>().AsSelf();
			builder.RegisterType<EntitySubdivisionForUserPermission>().AsSelf();
			builder.RegisterType<EntityUserPermissionExtended>().AsSelf();
			builder.RegisterType<EntityUserPermission>().AsSelf();
			builder.RegisterType<HierarchicalPresetUserPermission>().AsSelf();
			builder.RegisterType<UserWarehousePermission>().AsSelf();
			builder.RegisterType<EntityUserPermissionExtended>().AsSelf();
			builder.RegisterType<UserPermissionNode>()
				.AsSelf()
				.AsImplementedInterfaces();

			#endregion
		}
	}
}
