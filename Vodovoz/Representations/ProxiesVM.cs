using System;
using System.Collections.Generic;
using Gtk;
using NHibernate.Transform;
using QSContacts;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Gtk.DataBindings;

namespace Vodovoz.ViewModel
{
	public class ProxiesVM : RepresentationModelBase<Proxy, ProxiesVMNode>
	{
		public IUnitOfWorkGeneric<Counterparty> CounterpartyUoW {
			get {
				return UoW as IUnitOfWorkGeneric<Counterparty>;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			Proxy proxyAlias = null;
			Counterparty counterpartyAlias = null;
			ProxiesVMNode resultAlias = null;
			Person personAlias = null;

			var proxieslist = UoW.Session.QueryOver<Proxy> (() => proxyAlias)
				.JoinAlias (c => c.Counterparty, () => counterpartyAlias)
				.JoinAlias (c => c.Persons, () => personAlias)
				.Where (() => counterpartyAlias.Id == CounterpartyUoW.Root.Id)
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

			SetItemsSource (proxieslist);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<ProxiesVMNode>.Create ()
			.AddColumn("Номер").SetDataProperty (node => node.Title)
			.AddColumn ("Начало действия").SetDataProperty (node => node.Start)
			.AddColumn ("Окончание действия").SetDataProperty (node => node.End)
			.AddColumn ("Кол-во лиц").SetDataProperty (node => node.PeopleCount)
			.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig;}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Proxy updatedSubject)
		{
			return CounterpartyUoW.Root.Id == updatedSubject.Counterparty.Id;
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			throw new NotImplementedException ();
		}

		#endregion

		public ProxiesVM (IUnitOfWorkGeneric<Counterparty> uow)
		{
			this.UoW = uow;
		}
	}
		
	public class ProxiesVMNode
	{
		public int Id{ get; set;}

		public string Number{ get; set;}

		public DateTime IssueDate{ get; set;}

		public DateTime StartDate{ get; set;}

		public DateTime EndDate{ get; set;}

		public string Title {
			get { return String.Format ("{0} от {1:d}", Number, IssueDate); }
		}
			
		public string Start { get { return String.Format ("{0:d}", StartDate); }}

		public string End { get { return String.Format ("{0:d}", EndDate); }}

		public string RowColor {
			get {
				if (DateTime.Today > EndDate)
					return "grey";
				if (DateTime.Today < StartDate)
					return "blue";
				return "black";
			}
		}
			
		public int PeopleCount{ get; set;}
	}
}

