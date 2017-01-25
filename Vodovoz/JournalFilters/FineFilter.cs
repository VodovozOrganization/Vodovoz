using System;
using QSOrmProject.RepresentationModel;

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

		public QSOrmProject.IUnitOfWork UoW
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}

