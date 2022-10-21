using QS.Tdi;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.TempAdapters
{
	public interface IGtkTabsOpener
	{
		void OpenOrderDlg(ITdiTab tab, int id);
		void OpenRouteListCreateDlg(ITdiTab tab, int id);
		ITdiTab OpenRouteListClosingDlg(ITdiTab master, int routelistId);
		ITdiTab OpenUndeliveredOrderDlg(ITdiTab tab, int id = 0, bool isForSalesDepartment = false);
		ITdiTab OpenUndeliveriesWithCommentsPrintDlg(ITdiTab tab, UndeliveredOrdersFilterViewModel filter);
		ITdiTab OpenCounterpartyDlg(ITdiTab master, int counterpartyId);
		void OpenTrackOnMapWnd(int routeListId);
		void OpenCashExpenseDlg(ITdiTab master, int employeeId, decimal balance, bool canChangeEmployee, ExpenseType expenseType);
		void OpenRouteListChangeGiveoutExpenceDlg(ITdiTab master, int employeeId, decimal balance, string description);
		ITdiTab OpenCounterpartyEdoTab(ITdiTab master, int counterpartyId);
	}
}
