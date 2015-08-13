using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	public partial class OrdersFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				enumcomboStatus.ItemsEnum = typeof(OrderStatus);
				var filter = new CounterpartyFilter (UnitOfWorkFactory.CreateWithoutRoot ());
				filter.RestrictCounterpartyType = CounterpartyType.customer;
				entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM (filter);
			}
		}

		public OrdersFilter (IUnitOfWork uow) : this ()
		{
			UoW = uow;
		}

		public OrdersFilter ()
		{
			this.Build ();
			IsFiltred = false;
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		public bool IsFiltred { get; private set; }

		#endregion

		public OrderStatus? RestrictStatus {
			get { return enumcomboStatus.SelectedItem as OrderStatus?; }
			set {
				enumcomboStatus.SelectedItem = value;
				enumcomboStatus.Sensitive = false;
			}
		}

		public Counterparty RestrictCounterparty {
			get { return entryreferenceClient.Subject as Counterparty; }
			set {
				entryreferenceClient.Subject = value;
				entryreferenceClient.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictDeliveryPoint {
			get { return entryreferencePoint.Subject as DeliveryPoint; }
			set {
				entryreferencePoint.Subject = value;
				entryreferencePoint.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiodOrders.StartDateOrNull; }
			set {
				dateperiodOrders.StartDateOrNull = value;
				dateperiodOrders.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiodOrders.EndDateOrNull; }
			set {
				dateperiodOrders.EndDateOrNull = value;
				dateperiodOrders.Sensitive = false;
			}
		}

		protected void OnEnumcomboStatusEnumItemSelected (object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnEntryreferenceClientChanged (object sender, EventArgs e)
		{
			entryreferencePoint.Sensitive = RestrictCounterparty != null;
			if (RestrictCounterparty == null)
				entryreferencePoint.Subject = null;
			else {
				entryreferencePoint.Subject = null;
				entryreferencePoint.RepresentationModel = new ViewModel.DeliveryPointsVM (UoW, RestrictCounterparty);
			}
			OnRefiltered ();
		}

		protected void OnEntryreferencePointChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnDateperiodOrdersPeriodChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}
	}
}

