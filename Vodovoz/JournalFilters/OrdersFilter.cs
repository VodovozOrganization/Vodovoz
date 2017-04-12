using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
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
				enumcomboStatus.ItemsEnum = typeof (OrderStatus);
				var filter = new CounterpartyFilter (UnitOfWorkFactory.CreateWithoutRoot ());
				filter.RestrictIncludeCustomer = true;
				filter.RestrictIncludeSupplier = false;
				filter.RestrictIncludePartner = false;
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

			//Последние месяц назад и месяц вперед.
#if SHORT
			dateperiodOrders.StartDateOrNull = DateTime.Today;
			dateperiodOrders.EndDateOrNull = DateTime.Today;
#else
			dateperiodOrders.StartDateOrNull = DateTime.Today.AddMonths (-1);
			dateperiodOrders.EndDateOrNull = DateTime.Today.AddMonths (1);
#endif
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

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

		public bool RestrictOnlyWithoutCoodinates {
			get { return checkWithoutCoordinates.Active; }
			set {
				checkWithoutCoordinates.Active = value;
				checkWithoutCoordinates.Sensitive = false;
			}
		}

		bool? restrictSelfDelivery;

		public bool? RestrictSelfDelivery
		{
			get
			{
				return checkOnlySelfDelivery.Active ? true : restrictSelfDelivery;
			}
			set
			{
				restrictSelfDelivery = value;
				checkOnlySelfDelivery.Active = value == true;
				checkOnlySelfDelivery.Sensitive = false;
			}
		}

		bool? restrictWithoutSelfDelivery;

		public bool? RestrictWithoutSelfDelivery
		{
			get
			{
				return checkWithoutSelfDelivery.Active ? true : restrictWithoutSelfDelivery;
			}
			set
			{
				restrictWithoutSelfDelivery = value;
				checkWithoutSelfDelivery.Active = value == true;
				checkWithoutSelfDelivery.Sensitive = false;
			}
		}

		public int[] ExceptIds{ get; set; }

		protected void OnEntryreferenceClientChanged (object sender, EventArgs e)
		{
			entryreferencePoint.Sensitive = RestrictCounterparty != null;
			if (RestrictCounterparty == null)
				entryreferencePoint.Subject = null;
			else {
				entryreferencePoint.Subject = null;
				entryreferencePoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM (UoW, RestrictCounterparty);
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

		protected void OnEnumcomboStatusChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnCheckWithoutCoordinatesToggled(object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnCheckOnlySelfDeliveryToggled(object sender, EventArgs e)
		{
			OnRefiltered ();
			checkWithoutSelfDelivery.Sensitive = !checkOnlySelfDelivery.Active;
		}

		protected void OnCheckWithoutSelfDeliveryToggled (object sender, EventArgs e)
		{
			OnRefiltered ();
			checkOnlySelfDelivery.Sensitive = !checkWithoutSelfDelivery.Active;
		}
	}
}

