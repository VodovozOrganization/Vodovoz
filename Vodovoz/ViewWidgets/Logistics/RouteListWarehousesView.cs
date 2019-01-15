using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewWidgets.Logistics
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListWarehousesView : WidgetOnTdiTabBase
	{
		public RouteListWarehousesView()
		{
			this.Build();
			yTreeViewWarehouses.Selection.Changed += OnSelectionChanged;
		}

		GenericObservableList<LoadingUnloadingOperation> items;
		RouteList routeList;

		IUnitOfWorkGeneric<RouteList> routeListUoW;
		public IUnitOfWorkGeneric<RouteList> RouteListUoW {
			get => routeListUoW;
			set {
				if(routeListUoW == value)
					return;
				routeListUoW = value;
				ConfigureWidget();
			}
		}

		void ConfigureWidget()
		{
			routeList = routeListUoW.Root;

			hbxControls.Visible = routeList.WarehouseOperationType.HasValue && routeList.WarehouseOperationType.Value == OperationType.Unloading;
			UpdateData();
			UpdateColumns();

			routeList.ObservableAddresses.ElementAdded += (aList, aIdx) => UpdateWarehousesListOnLoading();
			routeList.ObservableAddresses.ElementRemoved += (aList, aIdx, aObject) => UpdateWarehousesListOnLoading();

			yTreeViewWarehouses.RowActivated += YTreeViewWarehouses_RowActivated;
			yTreeViewWarehouses.Reorderable = false;
		}

		void UpdateWarehousesListOnLoading()
		{
			var res = GetWarehouses(routeList);

			List<Warehouse> toAdd = res.ToList();
			List<Warehouse> toRemove = items.Where(o => o.OperType == OperationType.Loading)
				.Select(o => o.Warehouse)
				.ToList();

			foreach(Warehouse w in items.Where(o => o.OperType == OperationType.Loading).Select(o => o.Warehouse))
				if(res.Contains(w)) {
					toAdd.Remove(w);
					toRemove.Remove(w);
				} else
					toRemove.Add(w);

			foreach(Warehouse w in toAdd)
				items.Add(
					new LoadingUnloadingOperation {
						RouteList = routeList,
						Warehouse = w,
						OperType = OperationType.Loading
					}
				);

			foreach(Warehouse w in toRemove)
				items.Remove(items.FirstOrDefault(i => i.Warehouse == w));
		}

		void UpdateData()
		{
			switch(routeList.WarehouseOperationType) {
				case OperationType.Loading:
					items = routeList.ObservableWarehouseOperations;
					GetLoadingUnloadingOperations();
					break;
				case OperationType.Unloading:
					items = new GenericObservableList<LoadingUnloadingOperation>();
					foreach(var o in routeList.ObservableWarehouseOperations.Where(o => o.OperType == OperationType.Unloading))
						items.Add(o);
					break;
				default:
					items = routeList.ObservableWarehouseOperations;
					break;
			}
			yTreeViewWarehouses.ItemsDataSource = items;
		}

		void GetLoadingUnloadingOperations()
		{
			var warehouses = GetWarehouses(routeList);
			var warehousesFromOperations = routeList.ObservableWarehouseOperations
				.Where(o => o.OperType == OperationType.Loading)
				.Select(o => o.Warehouse);

			foreach(var wh in warehouses)
				if(!warehousesFromOperations.Contains(wh))
					routeList.ObservableWarehouseOperations.Add(
						new LoadingUnloadingOperation {
							RouteList = routeList,
							Warehouse = wh,
							OperType = OperationType.Loading
						}
					);
		}

		IList<Warehouse> GetWarehouses(RouteList rl)
		{
			var oi = rl.ObservableAddresses
					   .Where(i => !i.WasTransfered || i.NeedToReload)
					   .SelectMany(a => a.Order.OrderItems)
					   .Where(i => i.Nomenclature.Warehouse != null)
					   .Where(i => !i.Nomenclature.NoDelivey)
					   .Where(i => Nomenclature.GetCategoriesForShipment().Contains(i.Nomenclature.Category))
					   .Select(i => i.Nomenclature.Warehouse);

			var oe = rl.ObservableAddresses
					   .Where(i => !i.WasTransfered || i.NeedToReload)
					   .SelectMany(a => a.Order.OrderEquipments)
					   .Where(e => e.Nomenclature.Warehouse != null)
					   .Where(e => !e.Nomenclature.NoDelivey)
					   .Where(e => e.Direction == Domain.Orders.Direction.Deliver)
					   .Select(i => i.Nomenclature.Warehouse);

			//var list = Vodovoz.Repository.Store.WarehouseRepository.WarehouseForShipment(RouteListUoW, rl.Id);

			return oi.Union(oe).Distinct().ToList();
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			bool selected = yTreeViewWarehouses.Selection.CountSelectedRows() > 0;
			buttonDelete.Sensitive = selected;
		}

		void UpdateColumns()
		{
			var config = ColumnsConfigFactory.Create<LoadingUnloadingOperation>()
				.AddColumn("Склад")
					.AddTextRenderer(node => node.Warehouse.Name)
				.AddColumn("Статус")
					.AddTextRenderer()
					.AddSetter(
						(c, n) => {
							string type = n.OperType.GetEnumTitle();
							string state = n.IsComplete ? "завершена" : "не завершена";
							if(n.IsActive && n.RouteList.WarehouseOperationType.HasValue)
								state = "в процессе";
							c.Text = string.Format("{0} {1}", type, state);
						}
					)
				.RowCells()
					.AddSetter<CellRendererText>(
						(c, n) => {
							c.Foreground = "Black";
							if(n.IsActive)
								c.Foreground = "Green";
							if(n.IsComplete)
								c.Foreground = "Grey";
							if(!n.IsComplete && !n.RouteList.WarehouseOperationType.HasValue)
								c.Foreground = "Red";
						}
					)
				.Finish();

			yTreeViewWarehouses.ColumnsConfig = config;
		}

		void YTreeViewWarehouses_RowActivated(object o, RowActivatedArgs args)
		{
			if(routeList.WarehouseOperationType.HasValue) {
				var selectedOperation = yTreeViewWarehouses.GetSelectedObject<LoadingUnloadingOperation>();
				var operationAttribute = selectedOperation.OperType.GetAttributeOfEnumValue<AppellativeAttribute>(false);

				if(selectedOperation.IsActive || selectedOperation.IsComplete)
					return;
				bool response = MessageDialogHelper.RunQuestionWithTitleDialog(
					String.Format("Новая {0}?", operationAttribute.Nominative),
					String.Format("Отправить на {1} в '{0}'?", selectedOperation.Warehouse.Name, operationAttribute.Accusative)
				);
				if(!response)
					return;
				selectedOperation.IsActive = true;
				routeList.CurrentWarehouse = selectedOperation.Warehouse;
				foreach(LoadingUnloadingOperation oper in routeList.ObservableWarehouseOperations.Where(op => op.OperType == routeList.WarehouseOperationType.Value)) {
					if(selectedOperation != oper)
						oper.IsActive = false;
				}
			}
		}

		protected void OnBtnAddWarehouseClicked(object sender, EventArgs e)
		{
			var SelectWarehouseDlg = new OrmReference(typeof(Warehouse));
			SelectWarehouseDlg.Mode = OrmReferenceMode.MultiSelect;
			SelectWarehouseDlg.ObjectSelected += SelectWarehouseDlg_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectWarehouseDlg);
		}

		void SelectWarehouseDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(routeList.WarehouseOperationType.HasValue) {
				foreach(var warehouse in e.GetEntities<Warehouse>()) {
					if(routeList.ObservableWarehouseOperations.Any(x => x.Warehouse.Id == warehouse.Id && x.OperType == routeList.WarehouseOperationType.Value && !x.IsComplete))
						continue;
					routeList.ObservableWarehouseOperations.Add(
						new LoadingUnloadingOperation {
							OperType = routeList.WarehouseOperationType.Value,
							RouteList = routeList,
							Warehouse = warehouse
						}
					);
				}
				UpdateData();
			}
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var selectedOperation = yTreeViewWarehouses.GetSelectedObject<LoadingUnloadingOperation>();
			if(selectedOperation.IsActive || selectedOperation.IsComplete || routeList.WarehouseOperationType.HasValue && routeList.WarehouseOperationType.Value == OperationType.Loading)
				return;
			routeList.ObservableWarehouseOperations.Remove(selectedOperation);
			UpdateData();
		}
	}
}