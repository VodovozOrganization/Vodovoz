using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewWidgets.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DefectiveItemsReceptionView : WidgetOnDialogBase
	{
		GenericObservableList<DefectiveItemNode> defectiveList = new GenericObservableList<DefectiveItemNode>();

		public IList<DefectiveItemNode> Items => defectiveList;

		public void AddItem(DefectiveItemNode item)
		{
			defectiveList.Add(item);
		}

		public DefectiveItemsReceptionView()
		{
			this.Build();

			List<CullingCategory> types;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				types = uow.GetAll<CullingCategory>().OrderBy(c => c.Name).ToList();
			}
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			ytreeReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<DefectiveItemNode>()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Кол-во").AddNumericRenderer (node => node.Amount)
				.Adjustment (new Adjustment (0, 0, 9999, 1, 100, 0))
				.Editing (true)
				.AddColumn("Тип брака")
					.AddComboRenderer(x => x.TypeOfDefect)
					.SetDisplayFunc(x => x.Name)
					.FillItems(types)
					.AddSetter(
						(c, n) => {
							c.Editable = true;
							c.BackgroundGdk = n.TypeOfDefect == null
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("Источник\nбрака")
					.AddEnumRenderer(x => x.Source, true, new Enum[] { DefectSource.None })
					.AddSetter(
						(c, n) => {
							c.Editable = true;
							c.BackgroundGdk = n.Source == DefectSource.None
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("")
				.Finish ();

			ytreeReturns.ItemsDataSource = defectiveList;
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
				FillDefectiveListFromRoute();
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
					FillDefectiveListFromRoute();
				} else {
					defectiveList.Clear();
				}

			}
		}

		public new bool Sensitive {
			set => ytreeReturns.Sensitive = buttonAddNomenclature.Sensitive = value;
		}

		void FillDefectiveListFromRoute()
		{
			if(Warehouse == null || RouteList == null)
				return;

			DefectiveItemNode resultAlias = null;
			WarehouseMovementOperation warehouseMovementOperationAlias = null;
			CarUnloadDocument carUnloadDocumentAlias = null;
			CarUnloadDocumentItem carUnloadDocumentItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var defectiveItems = UoW.Session.QueryOver<CarUnloadDocumentItem>(() => carUnloadDocumentItemAlias)
									.Left.JoinAlias(() => carUnloadDocumentItemAlias.Document, () => carUnloadDocumentAlias)
									.Where(() => carUnloadDocumentAlias.RouteList.Id == RouteList.Id)
									.Left.JoinAlias(() => carUnloadDocumentItemAlias.MovementOperation, () => warehouseMovementOperationAlias)
									.Left.JoinAlias(() => warehouseMovementOperationAlias.Nomenclature, () => nomenclatureAlias)
									.Where(() => nomenclatureAlias.IsDefectiveBottle)
									.Where(
										Restrictions.Or(
											Restrictions.On(() => nomenclatureAlias.Warehouse).IsNull,
											Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Warehouse), Warehouse)
										   )
									   )
									.SelectList(
										list => list
										.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
										.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
										.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
				                        .Select(() => warehouseMovementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
				                        .Select(() => carUnloadDocumentItemAlias.MovementOperation).WithAlias(() => resultAlias.MovementOperation)
				                        .Select(() => carUnloadDocumentItemAlias.Source).WithAlias(() => resultAlias.Source)
				                        .Select(() => carUnloadDocumentItemAlias.TypeOfDefect).WithAlias(() => resultAlias.TypeOfDefect)
									   )
									.TransformUsing(Transformers.AliasToBean<DefectiveItemNode>())
									.List<DefectiveItemNode>();

			defectiveItems.ForEach(i => defectiveList.Add(i));
		}

		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e)
		{
			var SelectNomenclatureDlg = new OrmReference(
				QueryOver.Of<Nomenclature>().Where(x => x.IsDefectiveBottle)
			);
			SelectNomenclatureDlg.Mode = OrmReferenceMode.MultiSelect;
			SelectNomenclatureDlg.ObjectSelected += SelectNomenclatureDlg_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectNomenclatureDlg);
		}

		void SelectNomenclatureDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var nomenclature in e.GetEntities<Nomenclature>()) {
				defectiveList.Add(new DefectiveItemNode(nomenclature, 0));
			}
		}
	}

	public class DefectiveItemNode : PropertyChangedBase
	{
		public DefectiveItemNode(Nomenclature nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			this.amount = amount;
		}

		public DefectiveItemNode(WarehouseMovementOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			this.movementOperation = movementOperation;
		}

		public DefectiveItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.MovementOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public DefectiveItemNode(){}

		CarUnloadDocumentItem carUnloadDocumentItem = new CarUnloadDocumentItem();
		public virtual CarUnloadDocumentItem CarUnloadDocumentItem {
			get { return carUnloadDocumentItem; }
			set { SetField(ref carUnloadDocumentItem, value, () => CarUnloadDocumentItem); }
		}

		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		decimal amount;
		public virtual decimal Amount {
			get { return amount; }
			set { SetField(ref amount, value, () => Amount); }
		}

		WarehouseMovementOperation movementOperation;
		public virtual WarehouseMovementOperation MovementOperation {
			get { return movementOperation; }
			set { SetField(ref movementOperation, value, () => MovementOperation); }
		}

		CullingCategory typeOfDefect;
		[Display(Name = "Тип брака")]
		public virtual CullingCategory TypeOfDefect {
			get { return typeOfDefect; }
			set { SetField(ref typeOfDefect, value, () => TypeOfDefect); }
		}

		DefectSource source = DefectSource.Driver;
		[Display(Name = "Источник брака")]
		public virtual DefectSource Source {
			get { return source; }
			set { SetField(ref source, value, () => Source); }
		}
	}
}
