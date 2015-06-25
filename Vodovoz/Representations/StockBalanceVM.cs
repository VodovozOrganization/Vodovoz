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

		StockBalanceFilter filter;

		public StockBalanceFilter Filter {
			get {
				return filter;
			}
			set {
				if(filter != null)
					filter.Refiltered -= Filter_Refiltered;
				filter = value;
				if(filter != null)
					filter.Refiltered += Filter_Refiltered;
			}
		}

		void Filter_Refiltered (object sender, EventArgs e)
		{
			UpdateNodes ();
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			NodeStore.Clear ();

			Nomenclature nomenclatureAlias = null;
			StockBalanceVMNode resultAlias = null;
			GoodsMovementOperation operationAddAlias = null;
			GoodsMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<GoodsMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((filter == null || filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<GoodsMovementOperation> (o => o.IncomingWarehouse)) 
					: Restrictions.Eq (Projections.Property<GoodsMovementOperation> (o => o.IncomingWarehouse), filter.RestrictWarehouse))
				.Select (Projections.Sum<GoodsMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<GoodsMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((filter == null || filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<GoodsMovementOperation> (o => o.WriteoffWarehouse)) 
					: Restrictions.Eq (Projections.Property<GoodsMovementOperation> (o => o.WriteoffWarehouse), filter.RestrictWarehouse))
				.Select (Projections.Sum<GoodsMovementOperation> (o => o.Amount));

			var stocklist = uow.Session.QueryOver<Nomenclature> (() => nomenclatureAlias)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.SelectSubQuery (subqueryAdd).WithAlias(() => resultAlias.Append)
					.SelectSubQuery (subqueryRemove).WithAlias(() => resultAlias.Removed)
				)
				.TransformUsing(Transformers.AliasToBean<StockBalanceVMNode>())
				.List<StockBalanceVMNode>().Where(r => r.Amount > 0);

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
			//return uow.Root.Id == updatedSubject.Counterparty.Id;
			return false;
		}

		#endregion

		public StockBalanceVM () 
			: this(UnitOfWorkFactory.CreateWithoutRoot ()) 
		{}

		public StockBalanceVM (IUnitOfWork uow)
		{
			this.uow = uow;

			NodeStore = new NodeStore (NodeType);

			Columns.Add (new ColumnInfo { Name = "Номенклатура"}
				.SetDataProperty<StockBalanceVMNode> (node => node.NomenclatureName));
			Columns.Add (new ColumnInfo { Name = "Кол-во" }
				.SetDataProperty<StockBalanceVMNode> (node => node.CountText));
		}
	}

	[Gtk.TreeNode (ListOnly=true)]
	public class StockBalanceVMNode : TreeNode
	{

		public int Id{ get; set;}

		public int Append{ get; set;}

		public int Removed{ get; set;}

		public string Unit{ get; set;}

		[TreeNodeValue(Column = 0)]
		public string NomenclatureName { get; set;}

		[TreeNodeValue(Column = 1)]
		public string CountText { get { return String.Format ("{0} {1}", Amount, Unit); }}

		public int Amount { get { return Append - Removed; }}
	}
}

