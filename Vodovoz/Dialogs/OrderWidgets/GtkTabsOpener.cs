using QS.Dialog.Gtk;
using QS.Tdi;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

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

		public ITdiTab OpenUndeliveriesWithCommentsPrintDlg(ITdiTab tab, UndeliveredOrdersFilterViewModel filter)
		{
			return tab.TabParent.OpenTab(
					nameof(UndeliveriesWithCommentsPrintDlg),
					() => new UndeliveriesWithCommentsPrintDlg(filter)
					);
		}

		public ITdiTab OpenCounterpartyDlg(ITdiTab master, int counterpartyId)
		{
			return master.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Counterparty>(counterpartyId),
				() => new CounterpartyDlg(counterpartyId));
		}

		public void OpenCashExpenseDlg(ITdiTab master, int employeeId, decimal balance, IPermissionService permissionService,
			bool canChangeEmployee, ExpenseType expenseType)
		{
			var dlg = new CashExpenseDlg(permissionService);
			if(dlg.FailInitialize)
			{
				return;
			}

			dlg.ConfigureForSalaryGiveout(employeeId, balance, canChangeEmployee, expenseType);
			master.TabParent.AddTab(dlg, master);
		}
	}
}
