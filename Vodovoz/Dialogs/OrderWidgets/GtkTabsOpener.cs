using QS.Dialog.Gtk;
using QS.Tdi;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.Domain.Logistic;
namespace Vodovoz.Dialogs.OrderWidgets
{
	public class GtkTabsOpener : IGtkTabsOpenerForRouteListViewAndOrderView
	{
		public void OpenOrderDlg(ITdiTab tab, int id)
		{
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Order>(id),
				() => new OrderDlg(id)
			);
		}

		public void OpenCreateRouteListDlg(ITdiTab tab, int id)
		{
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(id),
				() => new RouteListCreateDlg(id)
			);
		}
	}
}