using System;
using QSOrmProject.RepresentationModel;
using QSOrmProject;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FineFilter : Gtk.Bin, IRepresentationFilter
	{
		public FineFilter()
		{
			this.Build();
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

