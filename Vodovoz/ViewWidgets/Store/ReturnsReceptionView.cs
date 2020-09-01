using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;
using Vodovoz.Services;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnsReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		GenericObservableList<ReceptionItemNode> ReceptionReturnsList = new GenericObservableList<ReceptionItemNode>();
		private ITerminalNomenclatureProvider terminalNomenclatureProvider = new BaseParametersProvider();

		public IList<ReceptionItemNode> Items => ReceptionReturnsList;

		public void AddItem(ReceptionItemNode item) => ReceptionReturnsList.Add(item);

		public ReturnsReceptionView()
		{
			this.Build();

			ytreeReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionItemNode>()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("№ Кулера").AddTextRenderer(node => node.Redhead)
					.AddSetter((cell, node) => cell.Editable = node.NomenclatureCategory == NomenclatureCategory.additional)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Amount, false)
					.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 100, 0))
					.AddSetter((cell, node) => cell.Editable = node.EquipmentId == 0)
				.AddColumn("Ожидаемое кол-во")
					.AddNumericRenderer(node => node.ExpectedAmount, false)
				.AddColumn("Цена закупки").AddNumericRenderer(node => node.PrimeCost).Digits(2).Editing()
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddTextRenderer(i => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("")
				.Finish();

			ytreeReturns.ItemsDataSource = ReceptionReturnsList;
		}

		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get => uow;
			set {
				if(uow == value)
					return;
				uow = value;
			}
		}

		Warehouse warehouse;
		public Warehouse Warehouse {
			get => warehouse;
			set {
				warehouse = value;
				FillListReturnsFromRoute(terminalNomenclatureProvider.GetNomenclatureIdForTerminal);
			}
		}

		RouteList routeList;
		public RouteList RouteList {
			get => routeList;
			set {
				if(routeList == value)
					return;
				routeList = value;
				if(routeList != null) {
					FillListReturnsFromRoute(terminalNomenclatureProvider.GetNomenclatureIdForTerminal);
				} else {
					ReceptionReturnsList.Clear();
				}
			}
		}

		public bool Sensitive {
			set => ytreeReturns.Sensitive = buttonAddNomenclature.Sensitive = value;
		}

		public IList<Equipment> AlreadyUnloadedEquipment;

		void FillListReturnsFromRoute(int terminalId)
		{
			if(Warehouse == null || RouteList == null)
				return;

			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Warehouse warehouseAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteListItem routeListItemToAlias = null;
			Employee employeeAlias = null;

			var returnableItems = UoW.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
				.Where(r => r.RouteList.Id == RouteList.Id)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Warehouses, () => warehouseAlias)
				.Left.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Left.JoinAlias(() => routeListItemAlias.TransferedTo, () => routeListItemToAlias)
				.Where(Restrictions.Or(
					Restrictions.On(() => warehouseAlias.Id).IsNull,
					Restrictions.Eq(Projections.Property(() => warehouseAlias.Id), Warehouse.Id)
				))
				.Where(() => nomenclatureAlias.Category != NomenclatureCategory.deposit)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.Int32, "SUM(IF(?1 = 'Canceled' OR ?1 = 'Overdue' OR (?1 = 'Transfered' AND ?2 = 1), ?3, 0))"),
					   NHibernateUtil.Int32,
					   Projections.Property(() => routeListItemAlias.Status),
					   Projections.Property(() => routeListItemToAlias.NeedToReload),
					   Projections.Property(() => orderItemsAlias.Count))
				   ).WithAlias(() => resultAlias.ExpectedAmount)
				)
				.TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
				.List<ReceptionItemNode>();

			var returnableEquipment = UoW.Session.QueryOver<RouteListItem>().Where(r => r.RouteList.Id == RouteList.Id)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.JoinAlias(() => orderEquipmentAlias.Equipment, () => equipmentAlias)
		        .JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
		        .Where(() => orderEquipmentAlias.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
		        .JoinAlias(() => nomenclatureAlias.Warehouses, () => warehouseAlias)
		        .Where(Restrictions.Or(
		            Restrictions.On(() => warehouseAlias.Id).IsNull,
		            Restrictions.Eq(Projections.Property(() => warehouseAlias.Id), Warehouse.Id)
		        ))
		        .Where(() => nomenclatureAlias.Category != NomenclatureCategory.deposit)
		        .SelectList(list => list
	                .Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.EquipmentId)
	                .Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
	                .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
		        )
		        .TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
		        .List<ReceptionItemNode>();

			var needTerminal = RouteList.Addresses.Any(x => x.Order.NeedTerminal);
			
			var returnableTerminal = uow.Session.QueryOver<EmployeeNomenclatureMovementOperation>()
			                            .Left.JoinAlias(x => x.Nomenclature, () => nomenclatureAlias)
			                            .Left.JoinAlias(x => x.Employee, () => employeeAlias)
			                            .JoinAlias(() => nomenclatureAlias.Warehouses, () => warehouseAlias)
			                            .Where(() => employeeAlias.Id == RouteList.Driver.Id)
			                            .And(() => nomenclatureAlias.Id == terminalId)
			                            .And(() => warehouseAlias.Id == Warehouse.Id)
			                            .SelectList(list => list
			                                                .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
			                                                .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
			                                                .SelectSum(x => x.Amount).WithAlias(() => resultAlias.ExpectedAmount))
			                            .TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
			                            .SingleOrDefault<ReceptionItemNode>();

			foreach(var item in returnableItems) {
				if(!ReceptionReturnsList.Any(i => i.NomenclatureId == item.NomenclatureId))
					ReceptionReturnsList.Add(item);
			}

			foreach(var equipment in returnableEquipment) {
				if(!AlreadyUnloadedEquipment.Any(eq => eq.Id == equipment.EquipmentId))
					ReceptionReturnsList.Add(equipment);
			}

			if (returnableTerminal != null && needTerminal) {
				if (ReceptionReturnsList.All(i => i.NomenclatureId != returnableTerminal.NomenclatureId))
					ReceptionReturnsList.Add(returnableTerminal);
			}
		}

		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e)
		{
			var allowCategories = Nomenclature.GetCategoriesForGoods().Where(c => c != NomenclatureCategory.bottle && c != NomenclatureCategory.equipment).ToArray();
			var SelectNomenclatureDlg = new OrmReference(
				QueryOver.Of<Nomenclature>().Where(x => x.Category.IsIn(allowCategories))
			);
			SelectNomenclatureDlg.Mode = OrmReferenceMode.MultiSelect;
			SelectNomenclatureDlg.ObjectSelected += SelectNomenclatureDlg_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectNomenclatureDlg);
		}

		void SelectNomenclatureDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var nomenclature in e.GetEntities<Nomenclature>()) {
				if(Items.Any(x => x.NomenclatureId == nomenclature.Id))
					continue;
				ReceptionReturnsList.Add(new ReceptionItemNode(nomenclature, 0));
			}
		}
	}

	public class ReceptionItemNode : PropertyChangedBase
	{
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		int amount;
		public virtual int Amount {
			get => amount;
			set => SetField(ref amount, value, () => Amount);
		}

		int expectedAmount;
		public virtual int ExpectedAmount {
			get => expectedAmount;
			set => SetField(ref expectedAmount, value, () => ExpectedAmount);
		}

		int equipmentId;
		[PropertyChangedAlso("Serial")]
		public int EquipmentId {
			get => equipmentId;
			set => SetField(ref equipmentId, value, () => EquipmentId);
		}

		[Display(Name = "№ кулера")]
		public string Redhead {
			get => CarUnloadDocumentItem.Redhead;
			set {
				if(value != CarUnloadDocumentItem.Redhead)
					CarUnloadDocumentItem.Redhead = value;
			}
		}

		ServiceClaim serviceClaim;

		public virtual ServiceClaim ServiceClaim {
			get => serviceClaim;
			set => SetField(ref serviceClaim, value, () => ServiceClaim);
		}

		public Equipment NewEquipment { get; set; }
		public bool Returned {
			get => Amount > 0;
			set => Amount = value ? 1 : 0;
		}

		WarehouseMovementOperation movementOperation = new WarehouseMovementOperation();

		public virtual WarehouseMovementOperation MovementOperation {
			get => movementOperation;
			set => SetField(ref movementOperation, value, () => MovementOperation);
		}

		public ReceptionItemNode(Nomenclature nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			this.amount = amount;
		}

		public ReceptionItemNode(WarehouseMovementOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			this.movementOperation = movementOperation;
		}

		CarUnloadDocumentItem carUnloadDocumentItem = new CarUnloadDocumentItem();

		public virtual CarUnloadDocumentItem CarUnloadDocumentItem {
			get => carUnloadDocumentItem;
			set => SetField(ref carUnloadDocumentItem, value, () => CarUnloadDocumentItem);
		}

		public ReceptionItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.MovementOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public ReceptionItemNode() { }

		[Display(Name = "Цена")]
		public virtual decimal PrimeCost {
			get => MovementOperation.PrimeCost;
			set {
				if(value != MovementOperation.PrimeCost)
					MovementOperation.PrimeCost = value;
			}
		}

		public virtual decimal Sum => PrimeCost * Amount;
	}
}

