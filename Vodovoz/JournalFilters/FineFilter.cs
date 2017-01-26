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

	}
}

