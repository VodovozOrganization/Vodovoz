using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Widgets;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListsFilter : RepresentationFilterBase<RouteListsFilter>
	{
		protected override void ConfigureWithUow()
		{
			SetAndRefilterAtOnce(
				x => x.enumcomboStatus.ItemsEnum = typeof(RouteListStatus),
				x => x.yentryreferenceShift.SubjectType = typeof(DeliveryShift),
				x => x.yEnumCmbTransport.ItemsEnum = typeof(RLFilterTransport)
			);
		}

		public RouteListsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public RouteListsFilter()
		{
			this.Build();
		}

		public RouteListStatus? RestrictStatus {
			get { return enumcomboStatus.SelectedItem as RouteListStatus?; }
			set {
				enumcomboStatus.SelectedItemOrNull = value;
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

		private RouteListStatus[] onlyStatuses;

		public RouteListStatus[] OnlyStatuses {
			get {
				return onlyStatuses;
			}
			set {
				onlyStatuses = value;
				if(onlyStatuses != null) {
					RestrictStatus = null;
				}
			}
		}

		protected void OnEnumcomboStatusEnumItemSelected(object sender, ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodOrdersPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryreferenceShiftChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		public void SetFilterDates(DateTime? startDate, DateTime? endDate)
		{
			dateperiodOrders.StartDateOrNull = startDate;
			dateperiodOrders.EndDateOrNull = endDate;
		}

		public void SetFilterStatus(RouteListStatus? status)
		{
			enumcomboStatus.SelectedItem = status;
		}

		//возврат выбранного значения в списке ТС и засерение списка в случае программной установки значения
		public RLFilterTransport? RestrictTransport {
			get { return yEnumCmbTransport.SelectedItem as RLFilterTransport?; }
			set {
				yEnumCmbTransport.SelectedItemOrNull = value;
				yEnumCmbTransport.Sensitive = false;
			}
		}

		protected void OnYEnumCmbTransportChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}

	//значения для списка ТС
	public enum RLFilterTransport
	{
		[Display(Name = "Наёмники")]
		Mercenaries,
		[Display(Name = "Раскат")]
		Raskat,
		[Display(Name = "Ларгус")]
		Largus,
		[Display(Name = "ГАЗель")]
		GAZelle,
		[Display(Name = "Фура")]
		Waggon,
		[Display(Name = "Прочее")]
		Others
	}
}

