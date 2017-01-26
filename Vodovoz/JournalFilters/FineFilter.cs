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

		#endregion
		protected void OnYentryreferenceSubdivisionsChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}


	}
}

