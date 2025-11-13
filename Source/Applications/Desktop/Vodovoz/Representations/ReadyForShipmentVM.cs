using Autofac;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.RepresentationModel.GtkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModel
{
	public class ReadyForShipmentVM : QSOrmProject.RepresentationModel.RepresentationModelWithoutEntityBase<ReadyForShipmentVMNode>
	{
		public ReadyForShipmentVM() : this(ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot()) { }

		public ReadyForShipmentVM(IUnitOfWork uow) : base(
			typeof(RouteList),
			typeof(Domain.Orders.Order),
			typeof(CarLoadDocument)
		)
		{
			UoW = uow;
		}
		
		private readonly ISubdivisionRepository subdivisionRepository = ScopeProvider.Scope.Resolve<ISubdivisionRepository>();
		private readonly IRouteListRepository routeListRepository = ScopeProvider.Scope.Resolve<IRouteListRepository>();

		public ReadyForShipmentFilter Filter {
			get => RepresentationFilter as ReadyForShipmentFilter;
			set => RepresentationFilter = value;
		}

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature orderItemNomenclatureAlias = null, orderEquipmentNomenclatureAlias = null;

			RouteList routeListAlias = null;
			RouteListItem routeListAddressAlias = null;
			ReadyForShipmentVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;
			DeliveryShift shiftAlias = null;

			var orderitemsSubqury = QueryOver.Of(() => orderItemsAlias)
				.Where(() => orderItemsAlias.Order.Id == orderAlias.Id)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => orderItemNomenclatureAlias)
				.Select(i => i.Order);

			var orderEquipmentSubquery = QueryOver.Of(() => orderEquipmentAlias)
				.Where(() => orderEquipmentAlias.Order.Id == orderAlias.Id)
				.JoinAlias(() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => orderEquipmentNomenclatureAlias)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.Select(i => i.Order);

			var queryRoutes = UoW.Session.QueryOver(() => routeListAlias)
				.JoinAlias(rl => rl.Driver, () => employeeAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.Left.JoinAlias(rl => rl.Shift, () => shiftAlias)
				.Where(r => routeListAlias.Status == RouteListStatus.InLoading);
			
			var startDate = Filter.StartDate;
			var endDate = Filter.EndDate;

			if(startDate.HasValue)
			{
				queryRoutes.Where(() => routeListAlias.Date >= startDate);
			}

			if(endDate.HasValue)
			{
				queryRoutes.Where(() => routeListAlias.Date <= endDate);
			}

			if(Filter.Warehouse != null) {
				queryRoutes.JoinAlias(rl => rl.Addresses, () => routeListAddressAlias)
					.JoinAlias(() => routeListAddressAlias.Order, () => orderAlias)
					.Where(() => !routeListAddressAlias.WasTransfered || routeListAddressAlias.AddressTransferType == AddressTransferType.NeedToReload)
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
				   .Select(() => routeListAlias.Status).WithAlias(() => resultAlias.Status)
				   .Select(() => shiftAlias.Name).WithAlias(() => resultAlias.Shift)
				)
				.TransformUsing(Transformers.AliasToBean<ReadyForShipmentVMNode>())
				.List<ReadyForShipmentVMNode>();

			if(Filter.Warehouse != null) {
				var resultList = new List<ReadyForShipmentVMNode>();
				var routes = UoW.GetById<RouteList>(dirtyList.Select(x => x.Id));
				foreach(var dirty in dirtyList) {
					var route = routes.First(x => x.Id == dirty.Id);
					var inLoaded = routeListRepository.AllGoodsLoaded(UoW, route);
					var goodsAndEquips = routeListRepository.GetGoodsAndEquipsInRL(UoW, route, subdivisionRepository, Filter.Warehouse);

					bool closed = true;
					foreach(var rlItem in goodsAndEquips) {
						var loaded = inLoaded.FirstOrDefault(x => x.NomenclatureId == rlItem.NomenclatureId);
						if(loaded == null || loaded.Amount < rlItem.Amount) {
							closed = false;
							break;
						}
					}

					if(!closed) {
						resultList.Add(dirty);
					}
				}

				SetItemsSource(resultList.OrderByDescending(x => x.Date).ToList());
			} else {
				SetItemsSource(dirtyList.OrderByDescending(x => x.Date).ToList());
			}
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForShipmentVMNode>
			.Create()
			.AddColumn("Тип").AddTextRenderer(node => node.TypeString)
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Водитель").AddTextRenderer(node => node.Driver)
			.AddColumn("Машина").AddTextRenderer(node => node.Car)
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
			.AddColumn("Смена").AddTextRenderer(node => node.Shift)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		protected override bool NeedUpdateFunc(object updatedSubject) => true;

		public override IEnumerable<IJournalPopupItem> PopupItems {
			get {
				var result = new List<IJournalPopupItem>();

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Отгрузка со склада",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<ReadyForShipmentVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null && selectedNode.Status == RouteListStatus.InLoading)
							Startup.MainWin.TdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
								() => new RouteListControlDlg(selectedNode.Id)
							);
					}
				));

				return result;
			}
		}

		#endregion
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

		public RouteListStatus Status { get; set; }

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

