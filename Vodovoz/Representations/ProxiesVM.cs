using System;
using System.Collections.Generic;
using Gtk;
using NHibernate.Transform;
using QSContacts;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;

namespace Vodovoz.ViewModel
{
	public class ProxiesVM : RepresentationModelBase<Proxy>
	{
		IUnitOfWorkGeneric<Counterparty> uow;

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			NodeStore.Clear ();

			Proxy proxyAlias = null;
			Counterparty counterpartyAlias = null;
			ProxiesVMNode resultAlias = null;
			Person personAlias = null;

			var proxieslist = uow.Session.QueryOver<Proxy> (() => proxyAlias)
				.JoinAlias (c => c.Counterparty, () => counterpartyAlias)
				.JoinAlias (c => c.Persons, () => personAlias)
				.Where (() => counterpartyAlias.Id == uow.Root.Id)
				.SelectList(list => list
					.SelectGroup(() => proxyAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => proxyAlias.Number).WithAlias(() => resultAlias.Number)
					.Select(() => proxyAlias.IssueDate).WithAlias(() => resultAlias.IssueDate)
					.Select(() => proxyAlias.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(() => proxyAlias.ExpirationDate).WithAlias(() => resultAlias.EndDate)
					.SelectCount(() => personAlias.Id ).WithAlias(() => resultAlias.PeopleCount)
				)
				.TransformUsing(Transformers.AliasToBean<ProxiesVMNode>())
				.List<ProxiesVMNode>();

			foreach (var item in proxieslist)
				NodeStore.AddNode (item);
		}
			
		public override Type NodeType {
			get { return typeof(ProxiesVMNode);}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Proxy updatedSubject)
		{
			return uow.Root.Id == updatedSubject.Counterparty.Id;
		}

		#endregion

		public ProxiesVM (IUnitOfWorkGeneric<Counterparty> uow)
		{
			this.uow = uow;

			NodeStore = new NodeStore (NodeType);

			Columns.Add (new ColumnInfo { Name = "Номер"}
				.SetDataProperty<ProxiesVMNode> (node => node.Title));
			Columns.Add (new ColumnInfo { Name = "Начало действия" }
				.SetDataProperty<ProxiesVMNode> (node => node.Start));
			Columns.Add (new ColumnInfo { Name = "Окончание действия" }
				.SetDataProperty<ProxiesVMNode> (node => node.End));
			Columns.Add (new ColumnInfo { Name = "Кол-во лиц" }
				.SetDataProperty<ProxiesVMNode> (node => node.PeopleCount));

			SetRowAttribute<ProxiesVMNode> ("foreground", node => node.RowColor);
		}
	}

	[Gtk.TreeNode (ListOnly=true)]
	public class ProxiesVMNode : TreeNode
	{

		public int Id{ get; set;}

		public string Number{ get; set;}

		public DateTime IssueDate{ get; set;}

		public DateTime StartDate{ get; set;}

		public DateTime EndDate{ get; set;}

		[TreeNodeValue(Column = 0)]
		public string Title {
			get { return String.Format ("{0} от {1:d}", Number, IssueDate); }
		}

		[TreeNodeValue(Column = 1)]
		public string Start { get { return String.Format ("{0:d}", StartDate); }}

		[TreeNodeValue(Column = 2)]
		public string End { get { return String.Format ("{0:d}", EndDate); }}

		[TreeNodeValue(Column = 3)]
		public string RowColor {
			get {
				if (DateTime.Today > EndDate)
					return "grey";
				if (DateTime.Today < StartDate)
					return "blue";
				return "black";
			}
		}

		[TreeNodeValue(Column = 4)]
		public int PeopleCount{ get; set;}
	}
}

