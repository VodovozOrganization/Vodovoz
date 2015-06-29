using Gtk;
using NHibernate;
using QSOrmProject;
using Vodovoz;
using System.Collections.Generic;
using Vodovoz.Domain;

public partial class MainWindow : Gtk.Window
{
	Action ActionNewOrder;
	Action ActionOrdersTable;
	Action ActionServiceTickets;
	Action ActionWarehouseDocuments;
	Action ActionWarehouseStock;
	Action ActionRouteListTable;

	public void BuildToolbarActions ()
	{
		#region Creating actions
		//Заказы
		ActionOrdersTable = new Action ("ActionOrdersTable", "Журнал заказов", null, "table");
		//Сервис
		ActionServiceTickets = new Action ("ActionServiceTickets", "Журнал заявок", null, "table");
		//Склад
		ActionWarehouseDocuments = new Action ("ActionWarehouseDocuments", "Журнал документов", null, "table");
		ActionWarehouseStock = new Action ("ActionWarehouseStock", "Складские остатки", null, "table");
		//Логистика
		ActionRouteListTable = new Action ("ActionRouteListTable", "Маршрутные листы", null, "table");
		#endregion
		#region Inserting actions to the toolbar
		ActionGroup w1 = new ActionGroup ("ToolbarActions");
		w1.Add (ActionOrdersTable, null);
		w1.Add (ActionServiceTickets, null);
		w1.Add (ActionWarehouseDocuments, null);
		w1.Add (ActionWarehouseStock, null);
		w1.Add (ActionRouteListTable, null);
		UIManager.InsertActionGroup (w1, 0);
		#endregion
		#region Creating events
		ActionOrdersTable.Activated += ActionOrdersTableActivated;
		ActionServiceTickets.Activated += ActionServiceTicketsActivated;
		ActionWarehouseDocuments.Activated += ActionWarehouseDocumentsActivated;
		ActionWarehouseStock.Activated += ActionWarehouseStock_Activated;
		ActionRouteListTable.Activated += ActionRouteListTable_Activated;
		#endregion
	}

	void ActionRouteListTable_Activated (object sender, System.EventArgs e)
	{
		//TODO FIXME Сделать нормальный вид.
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<RouteList> ();

		OrmReference refWin = new OrmReference (typeof(RouteList), session, criteria);
		tdiMain.AddTab (refWin);
	}

	void ActionWarehouseStock_Activated (object sender, System.EventArgs e)
	{
		var tab = new StockBalanceView ();
		tdiMain.AddTab (tab);
	}

	void ActionWarehouseDocumentsActivated (object sender, System.EventArgs e)
	{
		var tab = new WarehouseDocumentsView ();
		tdiMain.AddTab (tab);
	}

	void ActionServiceTicketsActivated (object sender, System.EventArgs e)
	{
		//ISession session = OrmMain.OpenSession ();
	}

	void ActionOrdersTableActivated (object sender, System.EventArgs e)
	{
		//TODO FIXME Сделать нормальный вид.
		ISession session = OrmMain.OpenSession ();
		var criteria = session.CreateCriteria<Order> ();

		OrmReference refWin = new OrmReference (typeof(Order), session, criteria);
		tdiMain.AddTab (refWin);
	}
}