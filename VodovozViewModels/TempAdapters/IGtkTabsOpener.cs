using QS.Tdi;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.TempAdapters
{
	public interface IGtkTabsOpener
	{
		void OpenOrderDlg(ITdiTab tab, int id);
		
		void OpenCreateRouteListDlg(ITdiTab tab, int id);
		
		ITdiTab OpenUndeliveredOrderDlg(ITdiTab tab, int id = 0);

		ITdiTab OpenUndeliveriesWithCommentsPrintDlg(ITdiTab tab, UndeliveredOrdersFilterViewModel filter);
	}
}