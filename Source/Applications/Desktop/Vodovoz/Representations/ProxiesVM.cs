using System;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using Vodovoz.Domain.Contacts;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure;

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
					.SelectCountDistinct(() => personAlias.Id ).WithAlias(() => resultAlias.PeopleCount)
					.Select(Projections.SqlFunction (
						new SQLFunctionTemplate (NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property (() => deliveryPointAlias.ShortAddress),
						Projections.Constant ("\n"))).WithAlias(() => resultAlias.DeliveryPoints)
				)
				.TransformUsing(Transformers.AliasToBean<ProxiesVMNode>())
				.List<ProxiesVMNode>();

			SetItemsSource (proxieslist);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <ProxiesVMNode>.Create ()
			.AddColumn("Номер").AddTextRenderer(node => node.Title)
			.AddColumn ("Начало действия").AddTextRenderer(node => node.Start)
			.AddColumn ("Окончание действия").AddTextRenderer(node => node.End)
			.AddColumn ("Кол-во лиц").AddNumericRenderer(node => node.PeopleCount)
			.AddColumn ("Точки доставки").AddTextRenderer(node => node.DeliveryPoints)
			.RowCells ().AddSetter<CellRendererText> ((c, n) =>
			{
				var color = GdkColors.PrimaryText;

				if(DateTime.Today > n.EndDate)
				{
					color = GdkColors.InsensitiveText;
				}

				if(DateTime.Today < n.StartDate)
				{
					color = GdkColors.InfoText;
				}

				c.ForegroundGdk = color;
			})
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
			
		public int PeopleCount{ get; set;}

		public string DeliveryPoints { get; set;}
	}
}

