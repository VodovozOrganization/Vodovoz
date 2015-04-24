public partial class MainWindow : Gtk.Window
{
	Gtk.Action ActionNewOrder;
	Gtk.Action ActionOrdersTable;
	Gtk.Action ActionServiceTikets;

	public void BuildToolbarActions ()
	{
		ActionNewOrder = new Gtk.Action ("ActionNewOrder", "Новый заказ", null, "gtk-new");
		ActionOrdersTable = new Gtk.Action ("ActionOrdersTable", "Журнал заказов", null, "table");
		ActionServiceTikets = new Gtk.Action ("ActionServiceTikets", "Журнал заявок", null, "table");
		Gtk.ActionGroup w1 = new Gtk.ActionGroup ("ToolbarActions");
		w1.Add (ActionNewOrder, null);
		w1.Add (ActionOrdersTable, null);
		w1.Add (ActionServiceTikets, null);
		UIManager.InsertActionGroup (w1, 0);
	}
}