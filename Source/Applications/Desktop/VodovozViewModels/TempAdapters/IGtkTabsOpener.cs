﻿using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tdi;
using System;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.TempAdapters
{
	public interface IGtkTabsOpener
	{
		ITdiTab CreateOrderDlg(bool? isForRetail, bool? isForSalesDepartment);
		ITdiTab CreateOrderDlg(int? orderId);
		void OpenOrderDlg(ITdiTab tab, int id);
		void OpenOrderDlgFromViewModelByNavigator(DialogViewModelBase from, int orderId);
		ITdiTab OpenOrderDlgByNavigatorForCreateFromOnlineOrder(DialogViewModelBase from, OnlineOrder onlineOrder);
		void OpenCopyLesserOrderDlg(ITdiTab tab, int copiedOrderId);
		ITdiTab OpenCopyOrderDlg(ITdiTab tab, int copiedOrderId);
		ITdiTab OpenRouteListKeepingDlg(ITdiTabParent tab, int routeListId);
		ITdiTab OpenRouteListKeepingDlg(ITdiTab tab, int routeListId);
		ITdiTab OpenRouteListKeepingDlg(ITdiTabParent tab, int routeListId, int[] selectedOrdersIds);
		ITdiTab OpenRouteListKeepingDlg(ITdiTab tab, int routeListId, int[] selectedOrdersIds);
		void OpenRouteListClosingDlgFromViewModelByNavigator(DialogViewModelBase from, int routeListId);
		ITdiTab OpenRouteListClosingDlg(ITdiTab master, int routelistId);
		ITdiTab OpenRouteListClosingDlg(ITdiTabParent master, int routelistId);
		ITdiTab OpenUndeliveredOrderDlg(ITdiTab tab, int id = 0, bool isForSalesDepartment = false);
		ITdiTab OpenUndeliveriesWithCommentsPrintDlg(ITdiTab tab, UndeliveredOrdersFilterViewModel filter);
		void OpenUndeliveredOrdersClassificationReport(UndeliveredOrdersFilterViewModel filter, bool withTransfer);
		ITdiTab OpenCounterpartyDlg(ITdiTab master, int counterpartyId);
		void OpenTrackOnMapWnd(int routeListId);
		ITdiTab OpenCounterpartyEdoTab(int counterpartyId, ITdiTab master = null);
		ITdiTab OpenIncomingWaterDlg(int incomingWaterId, ITdiTab master = null);
		ITdiTab OpenSelfDeliveryDocumentDlg(int selfDeliveryDocumentId, ITdiTab master = null);
		ITdiTab OpenCarLoadDocumentDlg(int carLoadDocumentId, ITdiTab master = null);
		ITdiTab OpenCarUnloadDocumentDlg(int carUnloadDocumentId, ITdiTab master = null);
		ITdiTab OpenShiftChangeWarehouseDocumentDlg(int shiftChangeWarehouseDocumentId, ITdiTab master = null);
		ITdiTab OpenRegradingOfGoodsDocumentDlg(int regradingOfGoodsDocumentId, ITdiTab master = null);
		ITdiTab OpenRouteListControlDlg(ITdiTabParent tabParent, int id);
		string GenerateDialogHashName<T>(int id) where T : IDomainObject;
		void OpenCarLoadDocumentDlg(ITdiTabParent tabParent, Action<CarLoadDocument, IUnitOfWork, int, int> fillCarLoadDocumentFunc, int routeListId, int warehouseId);
		void ShowTrackWindow(int id);
		void OpenOrderDlgAsSlave(ITdiTab tab, Order order);
		void SwitchOnTab(ITdiTab tab);
		ITdiTab FindPageByHash<T>(int id) where T : IDomainObject;
		bool FindAndSwitchOnTab<T>(int id) where T : IDomainObject;
	}
}
