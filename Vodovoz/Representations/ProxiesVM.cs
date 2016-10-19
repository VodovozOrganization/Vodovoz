using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate.Transform;
using QSContacts;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate;

namespace Vodovoz.ViewModel
{
	public class ProxiesVM : RepresentationModelEntityBase<Proxy, ProxiesVMNode>
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
			DeliveryPoint deliveryPointAlias = null;

			var proxieslist = UoW.Session.QueryOver<Proxy> (() => proxyAlias)
				.JoinAlias (c => c.Counterparty, () => counterpartyAlias)
				.JoinAlias (c => c.Persons, () => personAlias)
				.JoinAlias (c => c.DeliveryPoints, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where (() => counterpartyAlias.Id == CounterpartyUoW.Root.Id)
				.SelectList(list => list
					.SelectGroup(() => proxyAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => proxyAlias.Number).WithAlias(() => resultAlias.Number)
					.Select(() => proxyAlias.IssueDate).WithAlias(() => resultAlias.IssueDate)
					.Select(() => proxyAlias.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(() => proxyAlias.ExpirationDate).WithAlias(() => resultAlias.EndDate)
					.SelectCount(() => personAlias.Id ).WithAlias(() => resultAlias.PeopleCount)
					.Select(Projections.SqlFunction (
						new SQLFunctionTemplate (NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property (() => deliveryPointAlias.ShortAddress),
						Projections.Constant ("\n"))).WithAlias(() => resultAlias.DeliveryPoints)
				)
				.TransformUsing(Transformers.AliasToBean<ProxiesVMNode>())
				.List<ProxiesVMNode>();

			SetItemsSource (proxieslist);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <ProxiesVMNode>.Create ()
			.AddColumn("Номер").SetDataProperty (node => node.Title)
			.AddColumn ("Начало действия").SetDataProperty (node => node.Start)
			.AddColumn ("Окончание действия").SetDataProperty (node => node.End)
			.AddColumn ("Кол-во лиц").SetDataProperty (node => node.PeopleCount)
			.AddColumn ("Точки доставки").AddTextRenderer(node => node.DeliveryPoints)
			.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Proxy updatedSubject)
		{
			return CounterpartyUoW.Root.Id == updatedSubject.Counterparty.Id;
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

		public string DeliveryPoints { get; set;}
	}
}

