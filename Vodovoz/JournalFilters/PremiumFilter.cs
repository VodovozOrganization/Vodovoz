using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PremiumFilter : Gtk.Bin, IRepresentationFilter
	{
		public PremiumFilter()
		{
			this.Build();
		}

		public PremiumFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
			yentryreferenceSubdivisions.SubjectType = typeof(Subdivision);
		}

		#region IRepresentationFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered()
		{
			if(Refiltered != null)
				Refiltered(this, new EventArgs());
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

		public Subdivision RestrictionSubdivision {
			get { return yentryreferenceSubdivisions.Subject as Subdivision; }
			set {
				yentryreferenceSubdivisions.Subject = value;
				yentryreferenceSubdivisions.Sensitive = false;
			}
		}

		public DateTime? RestrictionPremiumDateStart {
			get { return dateperiodpickerPremiumDate.StartDateOrNull; }
			set {
				dateperiodpickerPremiumDate.StartDateOrNull = value;
				dateperiodpickerPremiumDate.Sensitive = false;
			}
		}

		public DateTime? RestrictionPremiumDateEnd {
			get { return dateperiodpickerPremiumDate.EndDateOrNull; }
			set {
				dateperiodpickerPremiumDate.EndDateOrNull = value;
				dateperiodpickerPremiumDate.Sensitive = false;
			}
		}

		#endregion
		protected void OnYentryreferenceSubdivisionsChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}


		protected void OnDateperiodpickerPremiumDatePeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
