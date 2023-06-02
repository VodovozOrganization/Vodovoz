using QS.Tdi;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.TempAdapters
{
	public interface IGtkTabsOpener
	{
		ITdiTab CreateOrderDlg(bool? isForRetail, bool? isForSalesDepartment);
		ITdiTab CreateOrderDlg(int? orderId);
		void OpenOrderDlg(ITdiTab tab, int id);
		void OpenCopyOrderDlg(ITdiTab tab, int copiedOrderId);
		void OpenRouteListCreateDlg(ITdiTab tab, int id);
		void OpenRouteListKeepingDlg(ITdiTab tab, int routeListId);
		void OpenRouteListKeepingDlg(ITdiTab tab, int routeListId, int[] selectedOrdersIds);
		ITdiTab OpenRouteListClosingDlg(ITdiTab master, int routelistId);
		ITdiTab OpenUndeliveredOrderDlg(ITdiTab tab, int id = 0, bool isForSalesDepartment = false);
		ITdiTab OpenUndeliveriesWithCommentsPrintDlg(ITdiTab tab, UndeliveredOrdersFilterViewModel filter);
		ITdiTab OpenCounterpartyDlg(ITdiTab master, int counterpartyId);
		void OpenTrackOnMapWnd(int routeListId);
		void OpenCashExpenseDlg(ITdiTab master, int employeeId, decimal balance, bool canChangeEmployee, ExpenseType expenseType);
		void OpenRouteListChangeGiveoutExpenceDlg(ITdiTab master, int employeeId, int routeListId, decimal balance, string description);
		ITdiTab OpenCounterpartyEdoTab(int counterpartyId, ITdiTab master = null);
		ITdiTab OpenIncomingWaterDlg(int incomingWaterId, ITdiTab master = null);
		ITdiTab OpenSelfDeliveryDocumentDlg(int selfDeliveryDocumentId, ITdiTab master = null);
		ITdiTab OpenCarLoadDocumentDlg(int carLoadDocumentId, ITdiTab master = null);
		ITdiTab OpenCarUnloadDocumentDlg(int carUnloadDocumentId, ITdiTab master = null);
		ITdiTab OpenShiftChangeWarehouseDocumentDlg(int shiftChangeWarehouseDocumentId, ITdiTab master = null);
		ITdiTab OpenRegradingOfGoodsDocumentDlg(int regradingOfGoodsDocumentId, ITdiTab master = null);
	}
}
