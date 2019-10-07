using QS.Tdi;

namespace Vodovoz.TempAdapters
{
	public interface IGtkTabsOpenerForRouteListViewAndOrderView : IGtkOrderDlgOpener, IGtkRouteListCreateDlgOpener
	{
	}

	public interface IGtkOrderDlgOpener
	{
		void OpenOrderDlg(ITdiTab tab, int id);
	}

	public interface IGtkRouteListCreateDlgOpener
	{
		void OpenCreateRouteListDlg(ITdiTab tab, int id);
	}
}