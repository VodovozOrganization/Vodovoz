using Gtk;

public partial class MainWindow : Gtk.Window
{
	Action ActionNewOrder;
	Action ActionOrdersTable;
	Action ActionServiceTickets;
	Action ActionWarehouseDocuments;

	public void BuildToolbarActions ()
	{
		ActionNewOrder = new Action ("ActionNewOrder", "Новый заказ", null, "gtk-new");
		ActionOrdersTable = new Action ("ActionOrdersTable", "Журнал заказов", null, "table");
		ActionServiceTickets = new Action ("ActionServiceTickets", "Журнал заявок", null, "table");
		ActionWarehouseDocuments = new Action ("ActionWarehouseDocuments", "Журнал документов", null, "table");
		ActionGroup w1 = new ActionGroup ("ToolbarActions");
		w1.Add (ActionNewOrder, null);
		w1.Add (ActionOrdersTable, null);
		w1.Add (ActionServiceTickets, null);
		w1.Add (ActionWarehouseDocuments, null);
		UIManager.InsertActionGroup (w1, 0);
	}
}