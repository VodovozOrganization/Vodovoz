using Gtk;
using QSOrmProject;
using QSTDI;
using Vodovoz;
using Vodovoz.ViewModel;

public partial class MainWindow : Window
{
	Action ActionOrdersTable;
	Action ActionServiceClaims;
	Action ActionWarehouseDocuments;
	Action ActionWarehouseStock;
	Action ActionClientBalance;
	Action ActionRouteListTable;
	Action ActionRouteListClosingTable;
	Action ActionRouteListKeeping;
	Action ActionRouteListMileageCheck;
	Action ActionRouteListTracking;
	Action ActionAddOrder;
	Action ActionReadyForShipment;
	Action ActionReadyForReception;
	Action ActionCashDocuments;
	Action ActionAccountableDebt;
	Action ActionUnclosedAdvances;
	Action ActionCashFlow;
	Action ActionRevision;
	Action ActionRevisionBottlesAndDeposits;
	Action ActionTransferBankDocs;
	Action ActionAccountingTable;
	Action ActionAccountFlow;
	Action ActionExportTo1c;

	public void BuildToolbarActions ()
	{
		#region Creating actions
		//Заказы
		ActionOrdersTable = new Action ("ActionOrdersTable", "Журнал заказов", null, "table");
		ActionAddOrder = new Action ("ActionAddOrder", "Новый заказ", null, "table");
		//Сервис
		ActionServiceClaims = new Action ("ActionServiceTickets", "Журнал заявок", null, "table");
		//Склад
		ActionWarehouseDocuments = new Action ("ActionWarehouseDocuments", "Журнал документов", null, "table");
		ActionReadyForShipment = new Action ("ActionReadyForShipment", "Готовые к погрузке", null, "table");
		ActionReadyForReception = new Action("ActionReadyForReception", "Готовые к разгрузке", null, "table");
		ActionWarehouseStock = new Action ("ActionWarehouseStock", "Складские остатки", null, "table");
		ActionClientBalance = new Action ("ActionClientBalance", "Оборудование у клиентов", null, "table");
		//Логистика
		ActionRouteListTable = new Action ("ActionRouteListTable", "Маршрутные листы", null, "table");
		ActionRouteListClosingTable = new Action("ActionRouteListClosingTable", "Закрытие маршрутных листов",null,"table");
		ActionRouteListTracking = new Action("ActionRouteListTracking", "Мониторинг машин",null,"table");
		ActionRouteListKeeping = new Action("ActionRouteListKeeping", "Ведение маршрутных листов",null,"table");
		ActionRouteListMileageCheck = new Action("ActionRouteListMileageCheck", "Контроль за километражом",null,"table");
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
		ActionRevisionBottlesAndDeposits = new Action ("RevisionBottlesAndDeposits", "Акт по бутылям/залогам", null, "table");
		#endregion
		#region Inserting actions to the toolbar
		ActionGroup w1 = new ActionGroup ("ToolbarActions");
		w1.Add (ActionOrdersTable, null);
		w1.Add (ActionAddOrder, null);
		w1.Add (ActionServiceClaims, null);
		w1.Add (ActionWarehouseDocuments, null);
		w1.Add (ActionReadyForShipment, null);
		w1.Add (ActionReadyForReception,null);
		w1.Add (ActionWarehouseStock, null);
		w1.Add (ActionClientBalance, null);
		w1.Add (ActionRouteListTable, null);
		w1.Add (ActionRouteListClosingTable, null);
		w1.Add (ActionRouteListKeeping, null);
		w1.Add (ActionRouteListTracking, null);
		w1.Add (ActionRouteListMileageCheck, null);
		w1.Add (ActionCashDocuments, null);
		w1.Add (ActionAccountableDebt, null);
		w1.Add (ActionUnclosedAdvances, null);
		w1.Add (ActionCashFlow, null);
		w1.Add (ActionRevision, null);
		w1.Add (ActionRevisionBottlesAndDeposits, null);
		w1.Add (ActionTransferBankDocs, null);
		w1.Add (ActionAccountingTable, null);
		w1.Add (ActionAccountFlow, null);
		w1.Add (ActionExportTo1c, null);
		UIManager.InsertActionGroup (w1, 0);
		#endregion
		#region Creating events
		ActionOrdersTable.Activated += ActionOrdersTableActivated;
		ActionAddOrder.Activated += ActionAddOrder_Activated;
		ActionServiceClaims.Activated += ActionServiceClaimsActivated;
		ActionWarehouseDocuments.Activated += ActionWarehouseDocumentsActivated;
		ActionReadyForShipment.Activated += ActionReadyForShipmentActivated;
		ActionReadyForReception.Activated+=ActionReadyForReceptionActivated;
		ActionWarehouseStock.Activated += ActionWarehouseStock_Activated;
		ActionClientBalance.Activated += ActionClientBalance_Activated;
		ActionRouteListTable.Activated += ActionRouteListTable_Activated;
		ActionRouteListClosingTable.Activated+= ActionRouteListClosingTable_Activated;
		ActionRouteListKeeping.Activated += ActionRouteListKeeping_Activated;
		ActionRouteListMileageCheck.Activated += ActionRouteListDistanceValidation_Activated;
		ActionRouteListTracking.Activated += ActionRouteListTracking_Activated;
		ActionCashDocuments.Activated += ActionCashDocuments_Activated;
		ActionAccountableDebt.Activated += ActionAccountableDebt_Activated;
		ActionUnclosedAdvances.Activated += ActionUnclosedAdvances_Activated;
		ActionCashFlow.Activated += ActionCashFlow_Activated;
		ActionRevision.Activated += ActionRevision_Activated;
		ActionRevisionBottlesAndDeposits.Activated += ActionRevisionBottlesAndDeposits_Activated;
		ActionTransferBankDocs.Activated += ActionTransferBankDocs_Activated;
		ActionAccountingTable.Activated += ActionAccountingTable_Activated;
		ActionAccountFlow.Activated += ActionAccountFlow_Activated;
		ActionExportTo1c.Activated += ActionExportTo1c_Activated;
		#endregion
	}

	void ActionRevisionBottlesAndDeposits_Activated (object sender, System.EventArgs e)
	{
		var widget = new Vodovoz.Reports.RevisionBottlesAndDeposits();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg (widget)
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
		);
	}
}