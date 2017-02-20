using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Logistic;
using Gamma.Widgets;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class RouteListsFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				enumcomboStatus.ItemsEnum = typeof(RouteListStatus);
				yentryreferenceShift.SubjectType = typeof(DeliveryShift);
			}
		}

		public RouteListsFilter (IUnitOfWork uow) : this ()
		{
			UoW = uow;
		}

		public RouteListsFilter ()
		{
			this.Build ();
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		#endregion

		public RouteListStatus? RestrictStatus {
			get { return enumcomboStatus.SelectedItem as RouteListStatus?; }
			set {
				enumcomboStatus.SelectedItem = value;
				enumcomboStatus.Sensitive = false;
			}
		}

		public DeliveryShift RestrictShift {
			get { return yentryreferenceShift.Subject as DeliveryShift; }
			set {
				yentryreferenceShift.Subject = value;
				yentryreferenceShift.Sensitive = false;
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

		protected void OnEnumcomboStatusEnumItemSelected (object sender, ItemSelectedEventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnDateperiodOrdersPeriodChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnYentryreferenceShiftChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		public void SetFilterDates(DateTime? startDate, DateTime? endDate)
		{
			dateperiodOrders.StartDateOrNull = startDate;
			dateperiodOrders.EndDateOrNull = endDate;
		}
	}
}

