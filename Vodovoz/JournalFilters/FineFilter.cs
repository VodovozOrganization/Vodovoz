using System;
using QSOrmProject.RepresentationModel;
using QSOrmProject;

namespace Vodovoz
{
	[OrmDefaultIsFiltered (true)]
	public partial class FineFilter : Gtk.Bin, IRepresentationFilter
	{
		public FineFilter()
		{
			this.Build();
		}

		public FineFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
			yentryreferenceSubdivisions.SubjectType = typeof(Subdivision);
		}
	
		#region IRepresentationFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		#endregion

		#region Свойства

		public  Subdivision RestrictionSubdivision {
			get {return yentryreferenceSubdivisions.Subject as Subdivision;}
			set {
				yentryreferenceSubdivisions.Subject = value;
				yentryreferenceSubdivisions.Sensitive = false;
			}
		}

		public  DateTime? RestrictionFineDateStart {
			get {return dateperiodpickerFineDate.StartDateOrNull;}
			set {
				dateperiodpickerFineDate.StartDateOrNull = value;
				dateperiodpickerFineDate.Sensitive = false;
			}
		}

		public  DateTime? RestrictionFineDateEnd {
			get {return dateperiodpickerFineDate.EndDateOrNull;}
			set {
				dateperiodpickerFineDate.EndDateOrNull = value;
				dateperiodpickerFineDate.Sensitive = false;
			}
		}

		public  DateTime? RestrictionRLDateStart {
			get {return dateperiodpickerRL.StartDateOrNull;}
			set {
				dateperiodpickerRL.StartDateOrNull = value;
				dateperiodpickerRL.Sensitive = false;
			}
		}

		public  DateTime? RestrictionRLDateEnd {
			get {return dateperiodpickerRL.EndDateOrNull;}
			set {
				dateperiodpickerRL.EndDateOrNull = value;
				dateperiodpickerRL.Sensitive = false;
			}
		}

		#endregion
		protected void OnYentryreferenceSubdivisionsChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}


		protected void OnDateperiodpickerFineDatePeriodChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnDateperiodpickerRLPeriodChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}
	}
}

