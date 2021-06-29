using QS.Dialog.Gtk;
using QS.Tdi;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;

namespace Vodovoz.Dialogs.OrderWidgets
{
	public class GtkTabsOpener : IGtkTabsOpener
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

		public ITdiTab OpenUndeliveredOrderDlg(ITdiTab tab, int id = 0)
		{
			return tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<UndeliveredOrder>(id),
				() => id > 0 ? new UndeliveredOrderDlg(id) : new UndeliveredOrderDlg()
			);
		}
	}
}