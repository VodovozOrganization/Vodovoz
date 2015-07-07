using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Operations;

namespace Vodovoz.ViewModel
{
	public class StockBalanceVM : RepresentationModelBase<Nomenclature>
	{
		IUnitOfWork uow;

		public StockBalanceFilter Filter {
			get {
				return RepresentationFilter as StockBalanceFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			NodeStore.Clear ();

			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			StockBalanceVMNode resultAlias = null;
			GoodsMovementOperation operationAddAlias = null;
			GoodsMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<GoodsMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((Filter == null || Filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<GoodsMovementOperation> (o => o.IncomingWarehouse)) 
					: Restrictions.Eq (Projections.Property<GoodsMovementOperation> (o => o.IncomingWarehouse), Filter.RestrictWarehouse))
				.Select (Projections.Sum<GoodsMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<GoodsMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((Filter == null || Filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<GoodsMovementOperation> (o => o.WriteoffWarehouse)) 
					: Restrictions.Eq (Projections.Property<GoodsMovementOperation> (o => o.WriteoffWarehouse), Filter.RestrictWarehouse))
				.Select (Projections.Sum<GoodsMovementOperation> (o => o.Amount));

			var stocklist = uow.Session.QueryOver<Nomenclature> (() => nomenclatureAlias)
				.JoinQueryOver(n => n.Unit, () => unitAlias)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.SelectSubQuery (subqueryAdd).WithAlias(() => resultAlias.Append)
					.SelectSubQuery (subqueryRemove).WithAlias(() => resultAlias.Removed)
				)
				.TransformUsing(Transformers.AliasToBean<StockBalanceVMNode>())
				.List<StockBalanceVMNode>().Where(r => r.Amount != 0);

			foreach (var item in stocklist)
				NodeStore.AddNode (item);
		}
			
		public override Type NodeType {
			get { return typeof(StockBalanceVMNode);}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Nomenclature updatedSubject)
		{
			throw new NotImplementedException ();
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			//FIXME Пока простая проверка.
			return true; //(updatedSubject is Nomenclature || updatedSubject is GoodsMovementOperation);
		}

		#endregion

		public StockBalanceVM (StockBalanceFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public StockBalanceVM () 
			: this(UnitOfWorkFactory.CreateWithoutRoot ()) 
		{}

		public StockBalanceVM (IUnitOfWork uow) : base(typeof(Nomenclature), typeof(GoodsMovementOperation))
		{
			this.uow = uow;

			NodeStore = new NodeStore (NodeType);

			Columns.Add (new ColumnInfo { Name = "Номенклатура"}
				.SetDataProperty<StockBalanceVMNode> (node => node.NomenclatureName));
			Columns.Add (new ColumnInfo { Name = "Кол-во" }
				.SetDataProperty<StockBalanceVMNode> (node => node.CountText));

			SetRowAttribute<StockBalanceVMNode> ("foreground", node => node.RowColor);
		}
	}

	[Gtk.TreeNode (ListOnly=true)]
	public class StockBalanceVMNode : TreeNode
	{

		public int Id{ get; set;}

		public decimal Append{ get; set;}

		public decimal Removed{ get; set;}

		public string UnitName{ get; set;}

		public short UnitDigits{ get; set;}

		[TreeNodeValue(Column = 0)]
		public string NomenclatureName { get; set;}

		[TreeNodeValue(Column = 1)]
		public string CountText { get { return String.Format ("{0:" + String.Format ("F{0}", UnitDigits) + "} {1}", 
			Amount,
			UnitName);
		}}

		public decimal Amount { get { return Append - Removed; }}

		[TreeNodeValue(Column = 2)]
		public string RowColor {
			get {
				if (Amount < 0)
					return "red";
				else
					return "black";
			}
		}
	}
}

