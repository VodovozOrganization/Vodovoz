using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModel
{
	public class ReadyForShipmentVM : RepresentationModelWithoutEntityBase<ReadyForShipmentVMNode>
	{
		public ReadyForShipmentVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }

		public ReadyForShipmentVM(IUnitOfWork uow) : base(
			typeof(RouteList),
			typeof(Domain.Orders.Order),
			typeof(CarLoadDocument)
		)
		{
			this.UoW = uow;
		}

		public ReadyForShipmentFilter Filter {
			get => RepresentationFilter as ReadyForShipmentFilter;
			set => RepresentationFilter = value as IRepresentationFilter;
		}

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature orderItemNomenclatureAlias = null, orderEquipmentNomenclatureAlias = null;
			Warehouse warehouseAlias = null;

			RouteList routeListAlias = null;
			RouteListItem routeListAddressAlias = null;
			ReadyForShipmentVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;
			DeliveryShift shiftAlias = null;

			var orderitemsSubqury = QueryOver.Of(() => orderItemsAlias)
				.Where(() => orderItemsAlias.Order.Id == orderAlias.Id)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => orderItemNomenclatureAlias)
				.JoinAlias(() => orderItemNomenclatureAlias.Warehouses, () => warehouseAlias)
				.Where(() => Filter.RestrictWarehouse.Id == warehouseAlias.Id)
				.Select(i => i.Order);
			var orderEquipmentSubquery = QueryOver.Of(() => orderEquipmentAlias)
				.Where(() => orderEquipmentAlias.Order.Id == orderAlias.Id)
				.JoinAlias(() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => orderEquipmentNomenclatureAlias)
				.JoinAlias(() => orderEquipmentNomenclatureAlias.Warehouses, () => warehouseAlias)
				.Where(() => Filter.RestrictWarehouse.Id == warehouseAlias.Id && orderEquipmentAlias.Direction == Direction.Deliver)
				.Select(i => i.Order);

			var queryRoutes = UoW.Session.QueryOver(() => routeListAlias)
				.JoinAlias(rl => rl.Driver, () => employeeAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.Left.JoinAlias(rl => rl.Shift, () => shiftAlias)
				.Where(r => routeListAlias.Status == RouteListStatus.InLoading);

			if(Filter.RestrictWarehouse != null) {
				queryRoutes.JoinAlias(rl => rl.Addresses, () => routeListAddressAlias)
					.JoinAlias(() => routeListAddressAlias.Order, () => orderAlias)
					.Where(() => !routeListAddressAlias.WasTransfered || routeListAddressAlias.NeedToReload)
					.Where(new Disjunction()
						.Add(Subqueries.WhereExists(orderitemsSubqury))
						.Add(Subqueries.WhereExists(orderEquipmentSubquery))
					);
			}

			var dirtyList = queryRoutes.SelectList(list => list
				   .SelectGroup(() => routeListAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
				   .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.Car)
				   .Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => shiftAlias.Name).WithAlias(() => resultAlias.Shift)
				)
				.TransformUsing(Transformers.AliasToBean<ReadyForShipmentVMNode>())
				.List<ReadyForShipmentVMNode>();

			if(Filter.RestrictWarehouse != null) {
				List<ReadyForShipmentVMNode> resultList = new List<ReadyForShipmentVMNode>();
				var routes = UoW.GetById<RouteList>(dirtyList.Select(x => x.Id));
				foreach(var dirty in dirtyList) {
					var route = routes.First(x => x.Id == dirty.Id);
					var inLoaded = Repository.Logistics.RouteListRepository.AllGoodsLoaded(UoW, route);

					var goodsAndEquips = Repository.Logistics.RouteListRepository.GetGoodsAndEquipsInRL(UoW, route, Filter.RestrictWarehouse);

					bool closed = true;
					foreach(var rlItem in goodsAndEquips) {
						var loaded = inLoaded.FirstOrDefault(x => x.NomenclatureId == rlItem.NomenclatureId);
						if(loaded == null || loaded.Amount < rlItem.Amount) {
							closed = false;
							break;
						}
					}

					if(!closed)
						resultList.Add(dirty);
				}

				SetItemsSource(resultList.OrderByDescending(x => x.Date).ToList());
			} else
				SetItemsSource(dirtyList.OrderByDescending(x => x.Date).ToList());
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForShipmentVMNode>
			.Create()
			.AddColumn("Тип").SetDataProperty(node => node.TypeString)
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Водитель").SetDataProperty(node => node.Driver)
			.AddColumn("Машина").SetDataProperty(node => node.Car)
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
			.AddColumn("Смена").AddTextRenderer(node => node.Shift)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		protected override bool NeedUpdateFunc(object updatedSubject) => true;

		public override bool PopupMenuExist => true;

		RepresentationSelectResult[] lastMenuSelected;
		RouteList selectedRouteList;
		public override Menu GetPopupMenu(RepresentationSelectResult[] selected)
		{
			lastMenuSelected = selected;
			var routeListId = lastMenuSelected.Select(x => x.EntityId)
											  .FirstOrDefault();

			selectedRouteList = UoW.Session.QueryOver<RouteList>()
										   .Where(x => x.Id == routeListId)
										   .List()
										   .FirstOrDefault();
			Menu popupMenu = new Menu();

			MenuItem menuItemRouteListControlDlg = new MenuItem("Отгрузка со склада");
			menuItemRouteListControlDlg.Activated += MenuItemRouteListControlDlg_Activated;
			popupMenu.Add(menuItemRouteListControlDlg);
			return popupMenu;
		}
		#endregion

		void MenuItemRouteListControlDlg_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null && selectedRouteList.Status == RouteListStatus.InLoading)
				MainClass.MainWin.TdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(selectedRouteList.Id),
					() => new RouteListControlDlg(selectedRouteList.Id)
				);
		}
	}

	public class ReadyForShipmentVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public string TypeString => "Маршрутный лист";

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Driver => string.Format("{0} {1} {2}", LastName, Name, Patronymic);

		[UseForSearch]
		[SearchHighlight]
		public string Car { get; set; }

		public DateTime Date { get; set; }

		public string Shift { get; set; }
	}

}

