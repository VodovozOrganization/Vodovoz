using QS.Views.GtkUI;
using System;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsReportsView : WidgetViewBase<PacsReportsViewModel>
	{
		public PacsReportsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();
		}
	}
}
