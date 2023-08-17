using Dialogs.Logistic;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tdi;
using System;
using System.Linq;
using Vodovoz.Dialogs.DocumentDialogs;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Dialogs.OrderWidgets
{
	public class GtkTabsOpener : IGtkTabsOpener
	{
		private ITdiTab FindTabByTag(string tag) =>
			TDIMain.MainNotebook.Tabs.FirstOrDefault(x => x.TdiTab is TdiTabBase tab && tab.Tag?.ToString() == tag)?.TdiTab;

		public string GenerateDialogHashName<T>(int id)
			where T : IDomainObject => DialogHelper.GenerateDialogHashName<T>(id);

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
		
		public void OpenCopyLesserOrderDlg(ITdiTab tab, int copiedOrderId)
		{
			var dlg = new OrderDlg();
			dlg.CopyLesserOrderFrom(copiedOrderId);
			
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Order>(65656),
				() => dlg
			);
		}

		public ITdiTab OpenCopyOrderDlg(ITdiTab tab, int copiedOrderId)
		{
			var tag = $"NewCopyFromOrder_{copiedOrderId}_Dlg";

			var existsTab = FindTabByTag(tag);

			if(existsTab == null)
			{
				var dlg = new OrderDlg();
				dlg.CopyOrderFrom(copiedOrderId);
				dlg.Tag = tag;
				tab.TabParent.OpenTab(() => dlg, tab);
				return FindTabByTag(tag);
			}
			else
			{
				TDIMain.MainNotebook.CurrentPage = TDIMain.MainNotebook.PageNum(existsTab as OrderDlg);
				return existsTab;
			}
		}

		public ITdiTab OpenRouteListCreateDlg(ITdiTab tab) =>
			OpenRouteListCreateDlg(tab.TabParent);

		public ITdiTab OpenRouteListCreateDlg(ITdiTab tab, int id) =>
			OpenRouteListCreateDlg(tab.TabParent, id);

		public ITdiTab OpenRouteListCreateDlg(ITdiTabParent tab, int id) =>
			tab.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(id),
				() => CreateRouteListCreateDlg(id));

		public ITdiTab OpenRouteListCreateDlg(ITdiTabParent tab) =>
			tab.OpenTab(CreateRouteListCreateDlg);

		public ITdiTab CreateRouteListCreateDlg() => new RouteListCreateDlg();

		public ITdiTab CreateRouteListCreateDlg(int id) => new RouteListCreateDlg(id);

		public ITdiTab OpenRouteListKeepingDlg(ITdiTabParent tab, int routeListId) =>
			tab.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(routeListId),
				() => new RouteListKeepingDlg(routeListId));

		public ITdiTab OpenRouteListKeepingDlg(ITdiTabParent tab, int routeListId, int[] selectedOrdersIds) =>
			tab.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(routeListId),
				() => new RouteListKeepingDlg(routeListId, selectedOrdersIds));

		public ITdiTab OpenRouteListKeepingDlg(ITdiTab tab, int routeListId, int[] selectedOrdersIds) =>
			OpenRouteListKeepingDlg(tab.TabParent, routeListId, selectedOrdersIds);

		public ITdiTab OpenRouteListKeepingDlg(ITdiTab tab, int routeListId) =>
			OpenRouteListKeepingDlg(tab.TabParent, routeListId);

		public ITdiTab OpenRouteListClosingDlg(ITdiTab master, int routelistId) =>
			OpenRouteListClosingDlg(master.TabParent, routelistId);

		public ITdiTab OpenRouteListClosingDlg(ITdiTabParent tab, int routelistId) =>
			tab.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(routelistId),
				() => new RouteListClosingDlg(routelistId));

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

		public ITdiTab OpenUndeliveredOrdersClassificationReport(ITdiTab tab, UndeliveredOrdersFilterViewModel filter, bool withTransfer)
		{
			return tab.TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<UndeliveredOrdersClassificationReport>(),
				() => new QSReport.ReportViewDlg(new UndeliveredOrdersClassificationReport(filter, withTransfer)));
		}

		public ITdiTab OpenCounterpartyDlg(ITdiTab master, int counterpartyId)
		{
			return master.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Counterparty>(counterpartyId),
				() => new CounterpartyDlg(counterpartyId));
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

		public ITdiTab OpenIncomingWaterDlg(int incomingWaterId, ITdiTab master = null) =>
			OpenDialogTabFor<IncomingWaterDlg, IncomingWater>(incomingWaterId, master);

		public ITdiTab OpenSelfDeliveryDocumentDlg(int selfDeliveryDocumentId, ITdiTab master = null) =>
			OpenDialogTabFor<SelfDeliveryDocumentDlg, SelfDeliveryDocument>(selfDeliveryDocumentId, master);

		public ITdiTab OpenCarLoadDocumentDlg(int carLoadDocumentId, ITdiTab master = null) =>
			OpenDialogTabFor<CarLoadDocumentDlg, CarLoadDocument>(carLoadDocumentId, master);

		public ITdiTab OpenCarUnloadDocumentDlg(int carUnloadDocumentId, ITdiTab master = null) =>
			OpenDialogTabFor<CarUnloadDocumentDlg, CarUnloadDocument>(carUnloadDocumentId, master);

		public ITdiTab OpenShiftChangeWarehouseDocumentDlg(int shiftChangeWarehouseDocumentId, ITdiTab master = null) =>
			OpenDialogTabFor<ShiftChangeWarehouseDocumentDlg, ShiftChangeWarehouseDocument>(shiftChangeWarehouseDocumentId, master);

		public ITdiTab OpenRegradingOfGoodsDocumentDlg(int regradingOfGoodsDocumentId, ITdiTab master = null) =>
			OpenDialogTabFor<RegradingOfGoodsDocumentDlg, RegradingOfGoodsDocument>(regradingOfGoodsDocumentId, master);

		private ITdiTab OpenDialogTabFor<TDialog, TEntity>(int entityId, ITdiTab master = null)
			where TDialog : ITdiTab
			where TEntity : IDomainObject
		{
			var dlgType = typeof(TDialog);

			var constructorWithId = dlgType.GetConstructor(new[] { typeof(int) });

			var constructorParameters = constructorWithId.GetParameters();

			if(constructorWithId is null
				|| (constructorParameters.Length != 1)
				|| !(constructorParameters[0].Name.EndsWith("Id")
					|| constructorParameters[0].Name == "id"))
			{
				throw new InvalidOperationException("Нет конструктора принимающего только Id");
			}

			var entityDlg = (TDialog)Activator.CreateInstance(dlgType, entityId);

			if(master != null)
			{
				return master.TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<TEntity>(entityId),
					() => entityDlg);
			}

			TDIMain.MainNotebook.AddTab(entityDlg);

			return entityDlg;
		}

		public ITdiTab OpenRouteListControlDlg(ITdiTabParent tabParent, int id) =>
			tabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(id),
				() => new RouteListControlDlg(id));

		public void OpenCarLoadDocumentDlg(ITdiTabParent tabParent, Action<CarLoadDocument, IUnitOfWork, int, int> fillCarLoadDocumentFunc, int routeListId, int warehouseId)
		{
			var dlg = new CarLoadDocumentDlg();
			fillCarLoadDocumentFunc(dlg.Entity, dlg.UoW, routeListId, warehouseId);
			tabParent.OpenTab(() => dlg);
		}

		public void ShowTrackWindow(int id)
		{
			var track = new TrackOnMapWnd(id);
			track.Show();
		}

		public void OpenOrderDlgAsSlave(ITdiTab tab, Order order)
		{
			var dlg = new OrderDlg(order)
			{
				HasChanges = false
			};

			dlg.SetDlgToReadOnly();
			tab.TabParent.AddSlaveTab(tab, dlg);
		}
	}
}
