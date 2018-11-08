using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnsReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		GenericObservableList<ReceptionItemNode> ReceptionReturnsList = new GenericObservableList<ReceptionItemNode>();

		public IList<ReceptionItemNode> Items {
			get {
				return ReceptionReturnsList;
			}
		}

		public void AddItem(ReceptionItemNode item)
		{
			ReceptionReturnsList.Add(item);
		}

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
			get {
				return uow;
			}
			set {
				if(uow == value)
					return;
				uow = value;
			}
		}

		Warehouse warehouse;
		public Warehouse Warehouse {
			get {
				return warehouse;
			}
			set {
				warehouse = value;
				FillListReturnsFromRoute();
			}
		}

		RouteList routeList;
		public RouteList RouteList {
			get {
				return routeList;
			}
			set {
				if(routeList == value)
					return;
				routeList = value;
				if(routeList != null) {
					FillListReturnsFromRoute();
				} else {
					ReceptionReturnsList.Clear();
				}

			}
		}

		public bool Sensitive {
			set {
				ytreeReturns.Sensitive = buttonAddNomenclature.Sensitive = value;
			}
		}

		public IList<Equipment> AlreadyUnloadedEquipment;

		void FillListReturnsFromRoute()
		{
			if(Warehouse == null || RouteList == null)
				return;

			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			var returnableItems = UoW.Session.QueryOver<RouteListItem>().Where(r => r.RouteList.Id == RouteList.Id)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => nomenclatureAlias)
				.Where(Restrictions.Or(
					Restrictions.On(() => nomenclatureAlias.Warehouse).IsNull,
					Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Warehouse), Warehouse)
				))
				.Where(() => nomenclatureAlias.Category != NomenclatureCategory.rent
				   && nomenclatureAlias.Category != NomenclatureCategory.deposit)
				.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
				)
				.TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
				.List<ReceptionItemNode>();

			var returnableEquipment = UoW.Session.QueryOver<RouteListItem>().Where(r => r.RouteList.Id == RouteList.Id)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.JoinAlias(() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderEquipmentAlias.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
				.Where(Restrictions.Or(
					Restrictions.On(() => nomenclatureAlias.Warehouse).IsNull,
					Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Warehouse), Warehouse)
				))
				.Where(() => nomenclatureAlias.Category != NomenclatureCategory.rent
				   && nomenclatureAlias.Category != NomenclatureCategory.deposit)
				.SelectList(list => list
				   .Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.EquipmentId)
				   .Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
				)
				.TransformUsing(Transformers.AliasToBean<ReceptionItemNode>())
				.List<ReceptionItemNode>();

			foreach(var item in returnableItems) {
				if(!ReceptionReturnsList.Any(i => i.NomenclatureId == item.NomenclatureId))
					ReceptionReturnsList.Add(item);
			}

			foreach(var equipment in returnableEquipment) {
				if(!AlreadyUnloadedEquipment.Any(eq => eq.Id == equipment.EquipmentId))
					ReceptionReturnsList.Add(equipment);
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
			get { return amount; }
			set {
				SetField(ref amount, value, () => Amount);
			}
		}

		int equipmentId;
		[PropertyChangedAlso("Serial")]
		public int EquipmentId {
			get {
				return equipmentId;
			}
			set {
				SetField(ref equipmentId, value, () => EquipmentId);
			}
		}

		[Display(Name = "№ кулера")]
		public string Redhead {
			get { return CarUnloadDocumentItem.Redhead; }
			set {
				if(value != CarUnloadDocumentItem.Redhead)
					CarUnloadDocumentItem.Redhead = value;
			}
		}

		ServiceClaim serviceClaim;

		public virtual ServiceClaim ServiceClaim {
			get { return serviceClaim; }
			set {
				SetField(ref serviceClaim, value, () => ServiceClaim);
			}
		}

		public Equipment NewEquipment { get; set; }
		public bool Returned {
			get {
				return Amount > 0;
			}
			set {
				Amount = value ? 1 : 0;
			}
		}

		WarehouseMovementOperation movementOperation = new WarehouseMovementOperation();

		public virtual WarehouseMovementOperation MovementOperation {
			get { return movementOperation; }
			set { SetField(ref movementOperation, value, () => MovementOperation); }
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
			get { return carUnloadDocumentItem; }
			set { SetField(ref carUnloadDocumentItem, value, () => CarUnloadDocumentItem); }
		}

		public ReceptionItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.MovementOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public ReceptionItemNode() { }

		[Display(Name = "Цена")]
		public virtual decimal PrimeCost {
			get { return MovementOperation.PrimeCost; }
			set {
				if(value != MovementOperation.PrimeCost)
					MovementOperation.PrimeCost = value;
			}
		}

		public virtual decimal Sum => PrimeCost * Amount;
	}
}

