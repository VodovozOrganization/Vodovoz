﻿using QS.Dialog.Gtk;
using QS.Tdi;
using QS.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Dialogs.Logistic;
using Vodovoz.Domain.Employees;
using System;

namespace Vodovoz.Dialogs.OrderWidgets
{
	public class GtkTabsOpener : IGtkTabsOpener
	{
		public ITdiTab CreateOrderDlg(bool? isForRetail, bool? isForSalesDepartment) =>
			new OrderDlg { IsForRetail = isForRetail, IsForSalesDepartment = isForSalesDepartment};
		
		public ITdiTab CreateOrderDlg(int? orderId) => orderId.HasValue ? new OrderDlg(orderId.Value) : new OrderDlg();

		public void OpenOrderDlg(ITdiTab tab, int id)
		{
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Order>(id),
				() => CreateOrderDlg(id)
			);
		}
		
		public void OpenCopyOrderDlg(ITdiTab tab, int copiedOrderId)
		{
			var dlg = new OrderDlg();
			dlg.CopyLesserOrderFrom(copiedOrderId);
			
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Order>(65656),
				() => dlg
			);
		}

		public void OpenRouteListCreateDlg(ITdiTab tab, int id)
		{
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(id),
				() => new RouteListCreateDlg(id)
			);
		}
		
		public void OpenRouteListKeepingDlg(ITdiTab tab, int routeListId, int[] selectedOrdersIds)
		{
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(routeListId),
				() => new RouteListKeepingDlg(routeListId, selectedOrdersIds)
			);
		}

		public ITdiTab OpenRouteListClosingDlg(ITdiTab master, int routelistId)
		{
			return master.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(routelistId),
				() => new RouteListClosingDlg(routelistId)
			);
		}

		public ITdiTab OpenUndeliveredOrderDlg(ITdiTab tab, int id = 0, bool isForSalesDepartment = false)
		{
			return tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<UndeliveredOrder>(id),
				() => id > 0 ? new UndeliveredOrderDlg(id, isForSalesDepartment) : new UndeliveredOrderDlg(isForSalesDepartment)
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

		public void OpenCashExpenseDlg(ITdiTab master, int employeeId, decimal balance, bool canChangeEmployee, ExpenseType expenseType)
		{
			var dlg = new CashExpenseDlg();
			if(dlg.FailInitialize)
			{
				return;
			}

			dlg.ConfigureForSalaryGiveout(employeeId, balance, canChangeEmployee, expenseType);
			master.TabParent.AddTab(dlg, master);
		}

		public void OpenRouteListChangeGiveoutExpenceDlg(ITdiTab master, int employeeId, decimal balance, string description)
		{
			var dlg = new CashExpenseDlg();
			if(dlg.FailInitialize)
			{
				return;
			}

			dlg.ConfigureForRouteListChangeGiveout(employeeId, balance, description);
			master.TabParent.AddTab(dlg, master);
		}

		public void OpenTrackOnMapWnd(int routeListId)
		{
			var track = new TrackOnMapWnd(routeListId);
			track.Show();
		}

		public ITdiTab OpenCounterpartyEdoTab(int counterpartyId, ITdiTab master = null)
		{
			var counterpartyDlg = new CounterpartyDlg(counterpartyId);
			counterpartyDlg.ActivateEdoTab();

			if(master != null)
			{
				return master.TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<Counterparty>(counterpartyId),
					() => counterpartyDlg);
			}

			TDIMain.MainNotebook.AddTab(counterpartyDlg);

			return counterpartyDlg;
		}
	}
}
