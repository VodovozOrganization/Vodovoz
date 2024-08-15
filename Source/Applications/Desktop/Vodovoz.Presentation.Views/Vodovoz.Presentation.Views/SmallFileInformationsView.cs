using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.AttachedFiles;

namespace Vodovoz.Presentation.Views
{
	[ToolboxItem(true)]
	public partial class SmallFileInformationsView : WidgetViewBase<AttachedFileInformationsViewModel>
	{


		public SmallFileInformationsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();
		}
	}
}
