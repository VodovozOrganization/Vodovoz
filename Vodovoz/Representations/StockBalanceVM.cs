using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QSContacts;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Operations;

namespace Vodovoz.ViewModel
{
	public class StockBalanceVM : RepresentationModelBase<Nomenclature>
	{
		IUnitOfWork uow;

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			NodeStore.Clear ();

			Nomenclature nomenclatureAlias = null;
			Counterparty counterpartyAlias = null;
			StockBalanceVMNode resultAlias = null;
			GoodsMovementOperation operationAddAlias = null;
			GoodsMovementOperation operationRemoveAlias = null;
			Person personAlias = null;

			var subqueryAdd = QueryOver.Of<GoodsMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id && operationAddAlias.IncomingWarehouse != null)
				.Select (Projections.Sum<GoodsMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<GoodsMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id && operationRemoveAlias.WriteoffWarehouse != null)
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

		public StockBalanceVM (IUnitOfWork uow)
		{
			this.uow = uow;

			NodeStore = new NodeStore (NodeType);

			Columns.Add (new ColumnInfo { Name = "Номенклатура"}
				.SetDataProperty<StockBalanceVMNode> (node => node.NomenclatureName));
/*			Columns.Add (new ColumnInfo { Name = "Начало действия" }
				.SetDataProperty<StockBalanceVMNode> (node => node.Start));
			Columns.Add (new ColumnInfo { Name = "Окончание действия" }
				.SetDataProperty<StockBalanceVMNode> (node => node.End)); */
			Columns.Add (new ColumnInfo { Name = "Кол-во" }
				.SetDataProperty<StockBalanceVMNode> (node => node.CountText));

			//SetRowAttribute<StockBalanceVMNode> ("foreground", node => node.RowColor);
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

		//[TreeNodeValue(Column = 2)]
		//public string End { get { return String.Format ("{0:d}", EndDate); }}

	/*	[TreeNodeValue(Column = 3)]
		public string RowColor {
			get {
				if (DateTime.Today > EndDate)
					return "grey";
				if (DateTime.Today < StartDate)
					return "blue";
				return "black";
			}
		} */

		//[TreeNodeValue(Column = 4)]
		public int Amount { get { return Append - Removed; }}
	}
}

