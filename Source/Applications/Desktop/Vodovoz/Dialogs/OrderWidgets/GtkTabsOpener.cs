using Autofac;
using Dialogs.Logistic;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.Tdi;
using QS.ViewModels.Dialog;
using QSOrmProject;
using System;
using System.Linq;
using Vodovoz.Dialogs.DocumentDialogs;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.ReportsParameters.Orders;
using VodovozBusiness.Extensions;

namespace Vodovoz.Dialogs.OrderWidgets
{
	public class GtkTabsOpener : IGtkTabsOpener
	{
		private ITdiTab FindTabByTag(string tag) =>
			TDIMain.MainNotebook.Tabs.FirstOrDefault(x => x.TdiTab is TdiTabBase tab && tab.Tag?.ToString() == tag)?.TdiTab;

		public string GenerateDialogHashName<T>(int id)
			where T : IDomainObject => DialogHelper.GenerateDialogHashName<T>(id);

		public ITdiTab FindPageByHash<T>(int id)
			where T : IDomainObject
		{
			var tab = Startup.MainWin.TdiMain.FindTab(GenerateDialogHashName<T>(id));
			return tab;
		}

		public void SwitchOnTab(ITdiTab tab)
		{
			Startup.MainWin.TdiMain.SwitchOnTab(tab);
		}

		public bool FindAndSwitchOnTab<T>(int id)
			where T : IDomainObject
		{
			var tab = FindPageByHash<T>(id);

			if(tab == null)
			{
				return false;
			}

			SwitchOnTab(tab);
			return true;
		}

		public ITdiTab CreateOrderDlg(bool? isForRetail, bool? isForSalesDepartment) =>
			new OrderDlg { IsForRetail = isForRetail, IsForSalesDepartment = isForSalesDepartment };

		public ITdiTab CreateOrderDlg(int? orderId) => orderId.HasValue ? new OrderDlg(orderId.Value) : new OrderDlg();

		public void OpenOrderDlg(ITdiTab tab, int id)
		{
			tab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Order>(id),
				() => CreateOrderDlg(id)
			);
		}

		public void OpenOrderDlgFromViewModelByNavigator(DialogViewModelBase from, int orderId)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<OrderDlg, int>(from, orderId);
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

		public void OpenRouteListClosingDlgFromViewModelByNavigator(DialogViewModelBase from, int routeListId)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<RouteListClosingDlg, int>(from, routeListId);
		}

		public ITdiTab OpenRouteListClosingDlg(ITdiTab master, int routelistId) =>
			OpenRouteListClosingDlg(master.TabParent, routelistId);

		public ITdiTab OpenRouteListClosingDlg(ITdiTabParent tab, int routelistId) =>
			tab.OpenTab(
				DialogHelper.GenerateDialogHashName<RouteList>(routelistId),
				() => new RouteListClosingDlg(routelistId));

		public ITdiTab OpenUndeliveriesWithCommentsPrintDlg(ITdiTab tab, UndeliveredOrdersFilterViewModel filter)
		{
			return tab.TabParent.OpenTab(
					nameof(UndeliveriesWithCommentsPrintDlg),
					() => new UndeliveriesWithCommentsPrintDlg(filter)
					);
		}

		public void OpenUndeliveredOrdersClassificationReport(UndeliveredOrdersFilterViewModel filter, bool withTransfer)
		{
			if(filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var scope = Startup.AppDIContainer.BeginLifetimeScope();
			var navigationManager = scope.Resolve<INavigationManager>();
			var rdlViewModel = navigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(UndeliveredOrdersClassificationReportViewModel));
			var undeliveredOrdersClassificationReportViewModel = (UndeliveredOrdersClassificationReportViewModel)rdlViewModel.ViewModel.ReportParametersViewModel;
			undeliveredOrdersClassificationReportViewModel.Load(filter, withTransfer);
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

		public ITdiTab CreateWarehouseDocumentOrmMainDialog(ITdiTabParent tabParent, Core.Domain.Warehouses.Documents.DocumentType type)
		{
			switch(type)
			{
				case Core.Domain.Warehouses.Documents.DocumentType.IncomingWater:
				case Core.Domain.Warehouses.Documents.DocumentType.SelfDeliveryDocument:
				case Core.Domain.Warehouses.Documents.DocumentType.CarLoadDocument:
				case Core.Domain.Warehouses.Documents.DocumentType.CarUnloadDocument:
					return tabParent.OpenTab(
						DialogHelper.GenerateDialogHashName(type.ToDocType(), 0),
						() => OrmMain.CreateObjectDialog(type.ToDocType()));
				default:
					throw new NotImplementedException("Тип документа не подерживается");
			}
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
