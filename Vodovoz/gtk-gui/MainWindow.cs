
public partial class MainWindow
{
	private Gtk.UIManager UIManager;

	private Gtk.Action ActionBaseMenu;

	private Gtk.Action dialogAuthenticationAction;

	private Gtk.Action ActionAbout;

	private Gtk.Action aboutAction;

	private Gtk.Action quitAction;

	private Gtk.RadioAction ActionOrders;

	private Gtk.RadioAction ActionServices;

	private Gtk.RadioAction ActionLogistics;

	private Gtk.RadioAction ActionStock;

	private Gtk.RadioAction ActionCash;

	private Gtk.RadioAction ActionAccounting;

	private Gtk.RadioAction ActionReports;

	private Gtk.RadioAction ActionArchive;

	private Gtk.Action ActionOrg;

	private Gtk.Action ActionBanksMenu;

	private Gtk.Action ActionBanksRF;

	private Gtk.Action ActionOrgMenu;

	private Gtk.Action ActionEmploey;

	private Gtk.Action ActionNationality;

	private Gtk.Action ActionEMailTypes;

	private Gtk.Action Action;

	private Gtk.Action ActionCounterpartyPost;

	private Gtk.Action ActionFreeRentPackage;

	private Gtk.Action ActionEquipment;

	private Gtk.Action ActionWarehouses;

	private Gtk.Action ActionCar;

	private Gtk.Action ActionColors;

	private Gtk.Action ActionUnits;

	private Gtk.Action ActionManufacturers;

	private Gtk.Action ActionEquipmentTypes;

	private Gtk.Action ActionNomenclature;

	private Gtk.Action ActionPhoneTypes;

	private Gtk.Action ActionTMC;

	private Gtk.Action ActionMenuLogistic;

	private Gtk.Action ActionCounterparty1;

	private Gtk.Action ActionCounterpartyHandbook;

	private Gtk.Action ActionPaidRentPackage;

	private Gtk.Action Action11;

	private Gtk.Action ActionDeliverySchedule;

	private Gtk.Action ActionLogisticsArea;

	private Gtk.Action ActionProductSpecification;

	private Gtk.Action ActionCullingCategory;

	private Gtk.Action Action12;

	private Gtk.Action ActionCommentTemplates;

	private Gtk.Action ActionLoad1c;

	private Gtk.Action ActionRouteColumns;

	private Gtk.Action ActionFuelType;

	private Gtk.Action ActionDeliveryShift;

	private Gtk.Action Action13;

	private Gtk.Action Action14;

	private Gtk.Action Action15;

	private Gtk.Action ActionDocTemplates;

	private Gtk.Action Action18;

	private Gtk.Action Action17;

	private Gtk.RadioAction ActionToolBarText;

	private Gtk.RadioAction ActionToolBarIcon;

	private Gtk.RadioAction ActionToolBarBoth;

	private Gtk.RadioAction ActionIconsExtraSmall;

	private Gtk.RadioAction ActionIconsSmall;

	private Gtk.RadioAction ActionIconsMiddle;

	private Gtk.RadioAction ActionIconsLarge;

	private Gtk.Action ActionDeliveryPoints;

	private Gtk.Action propertiesAction;

	private Gtk.Action ActionTransportationWagon;

	private Gtk.Action ActionRegrandingOfGoodsTempalte;

	private Gtk.Action Action31;

	private Gtk.Action ActionReportEmployees;

	private Gtk.Action ActionEmployeeFines;

	private Gtk.Action ActionReportsStock;

	private Gtk.Action ActionStockMovements;

	private Gtk.Action Subdivisions;

	private Gtk.Action ActionReportsSales;

	private Gtk.Action ActionSalesReport;

	private Gtk.Action ActionReportsDrivers;

	private Gtk.Action ActionDriverWages;

	private Gtk.Action ActionFuelReport;

	private Gtk.Action ActionReportOrders;

	private Gtk.Action ActionShortfallBattles;

	private Gtk.Action ActionWagesOperations;

	private Gtk.Action ActionEquipmentReport;

	private Gtk.Action ActionForwarderWageReport;

	private Gtk.RadioAction ActionStaff;

	private Gtk.Action ActionDriversWageBalance;

	private Gtk.Action ActionFineCommentTemplates;

	private Gtk.Action ActionDeliveriesLate;

	private Gtk.Action ActionRoutesListRegister;

	private Gtk.Action ActionDeliveryTimeReport;

	private Gtk.Action ActionCommentsForLogists;

	private Gtk.Action ActionOrdersByDistrict;

	private Gtk.Action ActionCompanyTrucks;

	private Gtk.Action Action53;

	private Gtk.Action Action55;

	private Gtk.Action ActionAddressDuplicetes;

	private Gtk.Action ActionLastOrderReport;

	private Gtk.Action ActionOrdersWithMinPriceLessThan;

	private Gtk.Action Action551;

	private Gtk.Action ActionOnLoadTime;

	private Gtk.Action actionSelfDeliveryReport;

	private Gtk.Action ActionDeliveryDaySchedule;

	private Gtk.Action ActionShipmentReport;

	private Gtk.Action ActionBottlesMovementRLReport;

	private Gtk.Action ActionReportService;

	private Gtk.Action ActionMastersVisitReport;

	private Gtk.Action ActionMileageReport;

	private Gtk.Action Action42;

	private Gtk.Action ActionSuburbWaterPrice;

	private Gtk.Action ActionDistanceFromCenter;

	private Gtk.Action Action38;

	private Gtk.Action ActionOrdersWithoutBottlesOperation;

	private Gtk.Action Action41;

	private Gtk.Action ActionHistoryLog;

	private Gtk.Action ActionDiscountReasons;

	private Gtk.Action Action59;

	private Gtk.Action Action45;

	private Gtk.Action ActionReportsBottles;

	private Gtk.Action ActionBottlesMovementSummaryReport;

	private Gtk.Action ActionDriveingCalls;

	private Gtk.Action ActionCounterpartyTag;

	private Gtk.Action ActionNotDeliveredOrders;

	private Gtk.Action Action47;

	private Gtk.Action ActionEmployeesBonuses;

	private Gtk.Action Action49;

	private Gtk.Action Action57;

	private Gtk.Action ActionLoad1cCounterpartyAndDeliveryPoints;

	private Gtk.Action ActionFolders1c;

	private Gtk.Action ActionOrderRegistry;

	private Gtk.Action Action50;

	private Gtk.Action ActionCardPayments;

	private Gtk.Action ActionCashierComments;

	private Gtk.Action ActionCameFrom;

	private Gtk.Action ActionProductGroups;

	private Gtk.Action ActionToOnlineStore;

	private Gtk.Action ActionSendedBills;

	private Gtk.Action ActionDefectiveItemsReport;

	private Gtk.Action ActionTrainee;

	private Gtk.Action ActionDeliveryPriceRules;

	private Gtk.Action Action51;

	private Gtk.Action OnLineAction;

	private Gtk.Action ActionLogistic;

	private Gtk.Action ActionOrdersByDistrictsAndDeliverySchedules;

	private Gtk.Action Action52;

	private Gtk.Action ActionAdministration;

	private Gtk.Action ActionTypesOfEntities;

	private Gtk.Action ActionUsers;

	private Gtk.Action ActionParameters;

	private Gtk.Action ActionService;

	private Gtk.Action Action58;

	private Gtk.Action ActionGeographicGroups;

	private Gtk.Action ActionSetDistrictsToDeliveryPoints;

	private Gtk.HBox hbox1;

	private Gtk.VBox vbox1;

	private Gtk.MenuBar menubarMain;

	private Gtk.HBox hbox2;

	private Gtk.Toolbar toolbarMain;

	private Vodovoz.UnreadedMessagesWidget unreadedMessagesWidget;

	private Gtk.Toolbar toolbarSub;

	private QS.Tdi.Gtk.TdiNotebook tdiMain;

	private Gtk.Statusbar statusbarMain;

	private Gtk.Label labelUser;

	private Gtk.ProgressBar progressStatus;

	private Gtk.Label labelStatus;

	private Vodovoz.SidePanel.InfoPanel infopanel;

	private void Build()
	{
	}
}
