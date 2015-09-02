using System;
using Gtk.DataBindings;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using System.Collections.Generic;
using System.Data.Bindings;
using NHibernate;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModel
{
	public class OrdersVM : RepresentationModelBase<Order, OrdersVMNode>
	{
		public OrdersFilter Filter {
			get {
				return RepresentationFilter as OrdersFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			OrdersVMNode resultAlias = null;
			Order orderAlias = null;
			Counterparty counterpartyAlias = null;

			var query = UoW.Session.QueryOver<Order> (() => orderAlias);

			if(Filter.RestrictStatus != null)
			{
				query.Where (o => o.OrderStatus == Filter.RestrictStatus);
			}

			if(Filter.RestrictCounterparty != null)
			{
				query.Where (o => o.Client == Filter.RestrictCounterparty);
			}

			if(Filter.RestrictDeliveryPoint != null)
			{
				query.Where (o => o.DeliveryPoint == Filter.RestrictDeliveryPoint);
			}

			if(Filter.RestrictStartDate != null)
			{
				query.Where (o => o.DeliveryDate >= Filter.RestrictStartDate);
			}

			if(Filter.RestrictEndDate != null)
			{
				query.Where (o => o.DeliveryDate <= Filter.RestrictEndDate.Value.AddDays (1).AddTicks (-1));
			}

			var result = query
				.JoinQueryOver (o => o.Client, () => counterpartyAlias)
				.SelectList (list => list
					.Select (() => orderAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => orderAlias.DeliveryDate).WithAlias (() => resultAlias.Date)
					.Select (() => orderAlias.OrderStatus).WithAlias (() => resultAlias.StatusEnum)
					.Select (() => counterpartyAlias.Name).WithAlias (() => resultAlias.Counterparty)
				)
				.TransformUsing (Transformers.AliasToBean<OrdersVMNode> ())
				.List<OrdersVMNode> ();

			SetItemsSource (result);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<OrdersVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("Дата").SetDataProperty (node => node.Date.ToString("d"))
			.AddColumn ("Статус").SetDataProperty (node => node.StatusEnum.GetEnumTitle ())
			.AddColumn ("Клиент").SetDataProperty (node => node.Counterparty)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Order updatedSubject)
		{
			return true;
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			throw new InvalidOperationException ();
		}

		#endregion

		public OrdersVM (OrdersFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public OrdersVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new OrdersFilter(UoW);
		}

		public OrdersVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
		}
	}

	public class OrdersVMNode
	{
		public int Id { get; set; }

		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }

		public string Counterparty { get; set; }
	}
}