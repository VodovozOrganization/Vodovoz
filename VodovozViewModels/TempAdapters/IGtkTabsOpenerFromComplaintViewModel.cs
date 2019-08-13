using QS.Tdi;

namespace Vodovoz.TempAdapters
{
	public interface IGtkTabsOpenerFromComplaintViewModel : IGtkOrderDlgOpener, IGtkRouteListCreateDlgOpener
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