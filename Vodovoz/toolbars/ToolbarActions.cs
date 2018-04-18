using Dialogs.Employees;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Representations;
using Vodovoz.ViewModel;

public partial class MainWindow : Window
{
	//Заказы
	Action ActionOrdersTable;
	Action ActionAddOrder;
	Action ActionLoadOrders;
	Action ActionDeliveryPrice;

	Action ActionServiceClaims;
	Action ActionWarehouseDocuments;
	Action ActionWarehouseStock;
	Action ActionClientBalance;
	//Логистика
	Action ActionRouteListTable;
	Action ActionAtWorks;
	Action ActionRouteListsAtDay;
	Action ActionRouteListsPrint;
	Action ActionRouteListClosingTable;
	Action ActionRouteListKeeping;
	Action ActionRouteListMileageCheck;
	Action ActionRouteListTracking;

	Action ActionReadyForShipment;
	Action ActionReadyForReception;
	Action ActionCashDocuments;
	Action ActionAccountableDebt;
	Action ActionUnclosedAdvances;
	Action ActionCashFlow;
	Action ActionFinesJournal;
	Action ActionPremiumJournal;
	Action ActionRevision;
	Action ActionRevisionBottlesAndDeposits;
	Action ActionReportDebtorsBottles;
	Action ActionTransferBankDocs;
	Action ActionAccountingTable;
	Action ActionAccountFlow;
	Action ActionExportTo1c;
	Action ActionResidue;
	Action ActionEmployeeWorkChart;
	Action ActionRouteListAddressesTransferring;
	Action ActionTransferOperationJournal;
	Action ActionScheduleRestrictedDistricts;

	public void BuildToolbarActions ()
	{
		#region Creating actions
		//Заказы
		ActionOrdersTable = new Action ("ActionOrdersTable", "Журнал заказов", null, "table");
		ActionAddOrder = new Action ("ActionAddOrder", "Новый заказ", null, "table");
		ActionLoadOrders = new Action("ActionLoadOrders", "Загрузить из 1С", null, "table");
		ActionDeliveryPrice = new Action("ActionDeliveryPrice", "Стоимость доставки", null, null);
		//Сервис
		ActionServiceClaims = new Action ("ActionServiceTickets", "Журнал заявок", null, "table");
		//Склад
		ActionWarehouseDocuments = new Action ("ActionWarehouseDocuments", "Журнал документов", null, "table");
		ActionReadyForShipment = new Action ("ActionReadyForShipment", "Готовые к погрузке", null, "table");
		ActionReadyForReception = new Action("ActionReadyForReception", "Готовые к разгрузке", null, "table");
		ActionWarehouseStock = new Action ("ActionWarehouseStock", "Складские остатки", null, "table");
		ActionClientBalance = new Action ("ActionClientBalance", "Оборудование у клиентов", null, "table");
		//Логистика
		ActionRouteListTable = new Action ("ActionRouteListTable", "Журнал МЛ", null, "table");
		ActionAtWorks = new Action("ActionAtWorks", "На работе", null, "table");
		ActionRouteListsAtDay = new Action ("ActionRouteListsAtDay", "Формирование МЛ", null, null);
		ActionRouteListsPrint = new Action("ActionRouteListsPrint", "Печать МЛ", null, "print");
		ActionRouteListClosingTable = new Action("ActionRouteListClosingTable", "Закрытие маршрутных листов",null,"table");
		ActionRouteListTracking = new Action("ActionRouteListTracking", "Мониторинг машин",null,"table");
		ActionRouteListKeeping = new Action("ActionRouteListKeeping", "Ведение маршрутных листов",null,"table");
		ActionRouteListMileageCheck = new Action("ActionRouteListMileageCheck", "Контроль за километражом",null,"table");
		ActionRouteListAddressesTransferring = new Action("ActionRouteListAddressesTransferring", "Перенос адресов", null, "table");
		//Касса
		ActionCashDocuments = new Action ("ActionCashDocuments", "Кассовые документы", null, "table");
		ActionAccountableDebt = new Action ("ActionAccountableDebt", "Долги сотрудников", null, "table");
		ActionUnclosedAdvances = new Action ("ActionUnclosedAdvances", "Незакрытые авансы", null, "table");
		ActionCashFlow = new Action ("ActionCashFlow", "Доходы и расходы", null, "table");
		//Бухгалтерия
		ActionTransferBankDocs = new Action ("ActionTransferBankDocs", "Загрузка из банк-клиента", null, "table");
		ActionExportTo1c = new Action ("ActionExportTo1c", "Выгрузка в 1с", null, "table");
		ActionAccountingTable = new Action ("ActionAccountingTable", "Операции по счету", null, "table");
		ActionAccountFlow = new Action ("ActionAccountFlow", "Доходы и расходы (безнал)", null, "table");
		ActionRevision = new Action ("ActionRevision", "Акт сверки", null, "table");
		//Архив
		ActionReportDebtorsBottles = new Action ("ReportDebtorsBottles", "Отчет по должникам тары", null, "table");
		ActionRevisionBottlesAndDeposits = new Action ("RevisionBottlesAndDeposits", "Акт по бутылям/залогам", null, "table");
		ActionResidue = new Action("ActionResidue", "Вввод остатков", null, "table");
		ActionTransferOperationJournal = new Action("ActionTransferOperationJournal", "Переносы между точками доставки", null, "table");
		//Кадры
		ActionEmployeeWorkChart = new Action("ActionEmployeeWorkChart", "График работы сотрудников", null, "table");
		ActionFinesJournal = new Action("ActionFinesJournal", "Штрафы", null, "table");
		ActionPremiumJournal = new Action("ActionPremiumJournal", "Премии", null, "table");
		ActionScheduleRestrictedDistricts = new Action("ActionScheduleRestrictedDistricts", "Районы с графиками доставки", null, "table");
		#endregion
		#region Inserting actions to the toolbar
		ActionGroup w1 = new ActionGroup("ToolbarActions");
		//Заказы
		w1.Add (ActionOrdersTable, null);
		w1.Add (ActionAddOrder, null);
		w1.Add (ActionLoadOrders, null);
		w1.Add(ActionDeliveryPrice, null);
		//
		w1.Add (ActionServiceClaims, null);
		w1.Add (ActionWarehouseDocuments, null);
		w1.Add (ActionReadyForShipment, null);
		w1.Add (ActionReadyForReception,null);
		w1.Add (ActionWarehouseStock, null);
		w1.Add (ActionClientBalance, null);
		//Логистика
		w1.Add (ActionRouteListTable, null);
		w1.Add(ActionAtWorks, null);
		w1.Add (ActionRouteListsAtDay, null);
		w1.Add(ActionRouteListsPrint, null);
		w1.Add (ActionRouteListClosingTable, null);
		w1.Add (ActionRouteListKeeping, null);
		w1.Add (ActionRouteListTracking, null);
		w1.Add (ActionRouteListMileageCheck, null);

		w1.Add (ActionCashDocuments, null);
		w1.Add (ActionAccountableDebt, null);
		w1.Add (ActionUnclosedAdvances, null);
		w1.Add (ActionCashFlow, null);
		w1.Add (ActionFinesJournal, null);
		w1.Add (ActionPremiumJournal, null);
		w1.Add (ActionRevision, null);
		w1.Add (ActionRevisionBottlesAndDeposits, null);
		w1.Add (ActionReportDebtorsBottles, null);
		w1.Add (ActionTransferBankDocs, null);
		w1.Add (ActionAccountingTable, null);
		w1.Add (ActionAccountFlow, null);
		w1.Add (ActionExportTo1c, null);
		w1.Add(ActionResidue, null);
		w1.Add(ActionEmployeeWorkChart, null);
		w1.Add(ActionRouteListAddressesTransferring, null);
		w1.Add(ActionTransferOperationJournal, null);
		w1.Add(ActionScheduleRestrictedDistricts, null);
		UIManager.InsertActionGroup (w1, 0);
		#endregion
		#region Creating events
		//Заказы
		ActionOrdersTable.Activated += ActionOrdersTableActivated;
		ActionAddOrder.Activated += ActionAddOrder_Activated;
		ActionLoadOrders.Activated += ActionLoadOrders_Activated;
		ActionDeliveryPrice.Activated += ActionDeliveryPrice_Activated;

		ActionServiceClaims.Activated += ActionServiceClaimsActivated;
		ActionWarehouseDocuments.Activated += ActionWarehouseDocumentsActivated;
		ActionReadyForShipment.Activated += ActionReadyForShipmentActivated;
		ActionReadyForReception.Activated+=ActionReadyForReceptionActivated;
		ActionWarehouseStock.Activated += ActionWarehouseStock_Activated;
		ActionClientBalance.Activated += ActionClientBalance_Activated;
		//Логистика
		ActionRouteListTable.Activated += ActionRouteListTable_Activated;
		ActionAtWorks.Activated +=ActionAtWorks_Activated;
		ActionRouteListsAtDay.Activated += ActionRouteListsAtDay_Activated;
		ActionRouteListsPrint.Activated += ActionRouteListsPrint_Activated;;
		ActionRouteListClosingTable.Activated+= ActionRouteListClosingTable_Activated;
		ActionRouteListKeeping.Activated += ActionRouteListKeeping_Activated;
		ActionRouteListMileageCheck.Activated += ActionRouteListDistanceValidation_Activated;
		ActionRouteListTracking.Activated += ActionRouteListTracking_Activated;

		ActionCashDocuments.Activated += ActionCashDocuments_Activated;
		ActionAccountableDebt.Activated += ActionAccountableDebt_Activated;
		ActionUnclosedAdvances.Activated += ActionUnclosedAdvances_Activated;
		ActionCashFlow.Activated += ActionCashFlow_Activated;
		ActionFinesJournal.Activated += ActionFinesJournal_Activated;
		ActionPremiumJournal.Activated += ActionPremiumJournal_Activated;
		ActionRevision.Activated += ActionRevision_Activated;
		ActionRevisionBottlesAndDeposits.Activated += ActionRevisionBottlesAndDeposits_Activated;
		ActionReportDebtorsBottles.Activated += ActionReportDebtorsBottles_Activated;
		ActionTransferBankDocs.Activated += ActionTransferBankDocs_Activated;
		ActionAccountingTable.Activated += ActionAccountingTable_Activated;
		ActionAccountFlow.Activated += ActionAccountFlow_Activated;
		ActionExportTo1c.Activated += ActionExportTo1c_Activated;
		ActionResidue.Activated += ActionResidueActivated;
		ActionEmployeeWorkChart.Activated += ActionEmployeeWorkChart_Activated;
		ActionRouteListAddressesTransferring.Activated += ActionRouteListAddressesTransferring_Activated;
		ActionTransferOperationJournal.Activated += ActionTransferOperationJournal_Activated;
		ActionScheduleRestrictedDistricts.Activated += ActionScheduleRestrictedDistricts_Activated;
		#endregion
	}

	void ActionRouteListsPrint_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<PrintRouteDocumentsDlg>(),
			() => new PrintRouteDocumentsDlg()
		);
	}

	void ActionRouteListAddressesTransferring_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListAddressesTransferringDlg>(),
			() => new RouteListAddressesTransferringDlg ()
		);
	}

	void ActionEmployeeWorkChart_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<EmployeeWorkChartDlg>(),
			() => new EmployeeWorkChartDlg ()
		);
	}
		
	void ActionLoadOrders_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<LoadFrom1cDlg>(),
			() => new LoadFrom1cDlg ()
		);
	}

	void ActionRevisionBottlesAndDeposits_Activated (object sender, System.EventArgs e)
	{
		var widget = new Vodovoz.Reports.RevisionBottlesAndDeposits();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg (widget)
		);
	}

	void ActionReportDebtorsBottles_Activated(object sender, System.EventArgs e)
	{
		var widget = new Vodovoz.ReportsParameters.ReportDebtorsBottles();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	void ActionAtWorks_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<AtWorksDlg>(),
			() => new AtWorksDlg()
		);
	}

	void ActionRouteListsAtDay_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RoutesAtDayDlg>(),
			() => new RoutesAtDayDlg ()
		);
	}

	void ActionAccountingTable_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<AccountingView>(),
			() => new AccountingView ()
		);
	}
		
	void ActionUnclosedAdvances_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<UnclosedAdvancesView>(),
			() => new UnclosedAdvancesView ()
		);
	}

	void ActionTransferBankDocs_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<LoadBankTransferDocumentDlg>(),
			() => new LoadBankTransferDocumentDlg ()
		);
	}

	void ActionCashFlow_Activated (object sender, System.EventArgs e)
	{
		var widget = new Vodovoz.Reports.CashFlow();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg (widget)
		);
	}

	void ActionFinesJournal_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<FinesVM>(),
			() => new ReferenceRepresentation (new FinesVM ()).CustomTabName("Журнал штрафов")
			.Buttons(QSMain.User.Permissions["can_delete_fines"] ? ReferenceButtonMode.CanAll : (ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanEdit))
		);
	}

	void ActionPremiumJournal_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<PremiumVM>(),
			() => new ReferenceRepresentation(new PremiumVM()).CustomTabName("Журнал премий")
			.Buttons(QSMain.User.Permissions["can_delete_fines"] ? ReferenceButtonMode.CanAll : (ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanEdit))
		);
	}

	void ActionRevision_Activated(object sender, System.EventArgs e)
	{
		var widget = new Vodovoz.Reports.Revision();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg (widget)
		);
	}

	void ActionAccountFlow_Activated (object sender, System.EventArgs e)
	{
		var widget = new Vodovoz.Reports.AccountFlow();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg (widget)
		);
	}
		
	void ActionExportTo1c_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ExportTo1cDialog>(),
			() => new ExportTo1cDialog ()
		);
	}

	void ActionAccountableDebt_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<AccountableDebts>(),
			() => new AccountableDebts ()
		);
	}

	void ActionRouteListTable_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<RouteListsVM>(),
			() => new ReferenceRepresentation(new RouteListsVM())
			.Buttons(QSMain.User.Permissions["can_delete"] ? ReferenceButtonMode.CanAll : (ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanEdit))
		);
	}

	void ActionRouteListClosingTable_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListClosingView>(),
			() => new RouteListClosingView ()
		);
	}

	void ActionRouteListTracking_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListTrackDlg>(),
			() => new RouteListTrackDlg ()
		);
	}

	void ActionRouteListKeeping_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListKeepingView>(),
			() => new RouteListKeepingView ()
		);
	}

	void ActionRouteListDistanceValidation_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<RouteListMileageCheckView>(),
			() => new RouteListMileageCheckView ()
		);
	}

	void ActionCashDocuments_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<CashDocumentsView>(),
			() => new CashDocumentsView ()
		);
	}

	void ActionReadyForShipmentActivated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ReadyForShipmentView>(),
			() => new ReadyForShipmentView ()
		);
	}

	void ActionReadyForReceptionActivated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ReadyForReceptionView>(),
			() => new ReadyForReceptionView ()
		);
	}

	void ActionClientBalance_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<ClientEquipmentBalanceVM>(),
			() => new ReferenceRepresentation (new ClientEquipmentBalanceVM (), "Оборудование у клиентов")
		);
	}

	void ActionAddOrder_Activated (object sender, System.EventArgs e)
	{		
		tdiMain.OpenTab(
			OrmMain.GenerateDialogHashName<Vodovoz.Domain.Orders.Order>(0),
			() => new OrderDlg ()
		);
	}

	void ActionWarehouseStock_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<StockBalanceView>(),
			() => new StockBalanceView ()
		);
	}

	void ActionWarehouseDocumentsActivated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<WarehouseDocumentsView>(),
			() => new WarehouseDocumentsView ()
		);
	}

	void ActionServiceClaimsActivated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ServiceClaimsView>(),
			() => new ServiceClaimsView ()
		);
	}

	void ActionOrdersTableActivated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<OrdersVM>(),
			() => new ReferenceRepresentation (new OrdersVM ()).CustomTabName("Журнал заказов")
			.Buttons(QSMain.User.Permissions["can_delete"] ? ReferenceButtonMode.CanAll : (ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanEdit))
		);
	}

	void ActionResidueActivated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<ResidueVM>(),
			() => new ReferenceRepresentation (new ResidueVM ()).CustomTabName("Журнал остатков")
		);
	}

	void ActionTransferOperationJournal_Activated (object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<TransferOperationsVM>(),
			() => new ReferenceRepresentation(new TransferOperationsVM()).CustomTabName("Переносы между точками доставки").Buttons(ReferenceButtonMode.CanAll)		);
	}

	void ActionDeliveryPrice_Activated(object sender, System.EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<DeliveryPriceDlg>(),
			() => new DeliveryPriceDlg()
		);
	}

	void ActionScheduleRestrictedDistricts_Activated (object sender, System.EventArgs e)
	{
		var tab = new ScheduleRestrictedDistrictsDlg();
		tdiMain.AddTab(tab);
	}
}