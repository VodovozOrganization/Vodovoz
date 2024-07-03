using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.Widgets.Print;
namespace Vodovoz.ViewWidgets.Users
{
	[ToolboxItem(true)]
	public partial class DocumentsPrinterSettingsView : WidgetViewBase<DocumentsPrinterSettingsViewModel>
	{
		public DocumentsPrinterSettingsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
		}
	}
}
