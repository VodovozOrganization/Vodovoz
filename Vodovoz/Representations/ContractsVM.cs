using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gtk;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;

namespace Vodovoz.ViewModel
{
	public class ContractsVM : IRepresentationModel
	{
		IUnitOfWorkGeneric<Counterparty> uow;

		#region IRepresentationModel implementation

		public void UpdateNodes ()
		{
			NodeStore.Clear ();

			CounterpartyContract contractAlias = null;
			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			ContractsVMNode resultAlias = null;
			AdditionalAgreement agreementAlias = null;

			var subquery = NHibernate.Criterion.QueryOver.Of<AdditionalAgreement>(() => agreementAlias)
				.Where(() => agreementAlias.Contract.Id == contractAlias.Id)
				.ToRowCountQuery();

			var contractslist = uow.Session.QueryOver<CounterpartyContract> (() => contractAlias)
				.JoinAlias (c => c.Counterparty, () => counterpartyAlias)
				.JoinAlias (c => c.Organization, () => organizationAlias)
				.Where (() => counterpartyAlias.Id == uow.Root.Id)
				.SelectList(list => list
					.Select(() => contractAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => contractAlias.IssueDate).WithAlias(() => resultAlias.IssueDate)
					.Select(() => contractAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(() => contractAlias.OnCancellation).WithAlias(() => resultAlias.OnCancellation)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Organization)
					.SelectSubQuery(subquery).WithAlias(() => resultAlias.AdditionalAgreements)
				)
				.TransformUsing(Transformers.AliasToBean<ContractsVMNode>())
				.List<ContractsVMNode>();

			foreach (var item in contractslist)
				NodeStore.AddNode (item);
		}

		public NodeStore NodeStore { get; set;}

		public Type NodeType {
			get { return typeof(ContractsVMNode);}
		}

		public Type ObjectType {
			get { return typeof(CounterpartyContract);}
		}

		private List<IColumnInfo> columns = new List<IColumnInfo> ();

		public List<IColumnInfo> Columns {
			get {
				return columns;
			}
		}

		#endregion

		public ContractsVM (IUnitOfWorkGeneric<Counterparty> uow)
		{
			this.uow = uow;

			NodeStore = new NodeStore (NodeType);

			Columns.Add (new ColumnInfo { Name = "Номер"}
				.SetDataProperty<ContractsVMNode> (node => node.Title));
			Columns.Add (new ColumnInfo { Name = "Организация" }
				.SetDataProperty<ContractsVMNode> (node => node.Organization));
			Columns.Add (new ColumnInfo { Name = "Кол-во доп. соглашений" }
				.SetDataProperty<ContractsVMNode> (node => node.AdditionalAgreements));

			SetRowAttribute<ContractsVMNode> ("foreground", node => node.RowColor);
		}

		void SetRowAttribute<TVMNode> (string attibuteName, Expression<Func<TVMNode, object>> propertyRefExpr)
		{
			Columns.ConvertAll (c => (ColumnInfo) c)
				.ForEach ((ColumnInfo column) => column.SetAttributeProperty(attibuteName, propertyRefExpr));
		}
	}

	[Gtk.TreeNode (ListOnly=true)]
	public class ContractsVMNode : TreeNode
	{

		public int Id{ get; set;}

		public DateTime IssueDate{ get; set;}

		public bool IsArchive{ get; set;}

		public bool OnCancellation{ get; set;}

		[TreeNodeValue(Column = 0)]
		public string Title {
			get { return String.Format ("{0} от {1:d}", Id, IssueDate); }
		}

		[TreeNodeValue(Column = 1)]
		public string Organization { get; set;}

		[TreeNodeValue(Column = 2)]
		public int AdditionalAgreements { get; set;}

		[TreeNodeValue(Column = 3)]
		public string RowColor {
			get {
				if (IsArchive)
					return "grey";
				if (OnCancellation)
					return "blue";
				return "black";

			}
		}

	}
}

