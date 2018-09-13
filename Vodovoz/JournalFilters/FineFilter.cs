using System;
using QSOrmProject.RepresentationModel;
using QSOrmProject;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class FineFilter : RepresentationFilterBase<FineFilter>
	{
		protected override void ConfigureWithUow()
		{
			yentryreferenceSubdivisions.SubjectType = typeof(Subdivision);
		}

		public FineFilter()
		{
			this.Build();
		}

		public FineFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		#region Свойства

		public Subdivision RestrictionSubdivision {
			get { return yentryreferenceSubdivisions.Subject as Subdivision; }
			set {
				yentryreferenceSubdivisions.Subject = value;
				yentryreferenceSubdivisions.Sensitive = false;
			}
		}

		public DateTime? RestrictionFineDateStart {
			get { return dateperiodpickerFineDate.StartDateOrNull; }
			set {
				dateperiodpickerFineDate.StartDateOrNull = value;
				dateperiodpickerFineDate.Sensitive = false;
			}
		}

		public DateTime? RestrictionFineDateEnd {
			get { return dateperiodpickerFineDate.EndDateOrNull; }
			set {
				dateperiodpickerFineDate.EndDateOrNull = value;
				dateperiodpickerFineDate.Sensitive = false;
			}
		}

		public DateTime? RestrictionRLDateStart {
			get { return dateperiodpickerRL.StartDateOrNull; }
			set {
				dateperiodpickerRL.StartDateOrNull = value;
				dateperiodpickerRL.Sensitive = false;
			}
		}

		public DateTime? RestrictionRLDateEnd {
			get { return dateperiodpickerRL.EndDateOrNull; }
			set {
				dateperiodpickerRL.EndDateOrNull = value;
				dateperiodpickerRL.Sensitive = false;
			}
		}

		public void SetFilterDates(DateTime? startDate, DateTime? endDate)
		{
			dateperiodpickerFineDate.StartDateOrNull = startDate;
			dateperiodpickerFineDate.EndDateOrNull = endDate;
		}

		#endregion
		protected void OnYentryreferenceSubdivisionsChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}


		protected void OnDateperiodpickerFineDatePeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodpickerRLPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

