using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Dialogs.Sales;

namespace Vodovoz.Views.Sale
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeoGroupView : EntityTabViewBase<GeoGroupViewModel, GeoGroup>
	{
		public GeoGroupView(GeoGroupViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
