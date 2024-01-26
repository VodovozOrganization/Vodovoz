using Gamma.GtkWidgets;
using QS.Views.GtkUI;
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

			var buttonMissingCallsReport = new yButton();
			buttonMissingCallsReport.CanFocus = true;
			buttonMissingCallsReport.Name = "buttonMissingCallsReport";
			buttonMissingCallsReport.UseUnderline = true;
			buttonMissingCallsReport.Label = Mono.Unix.Catalog.GetString("Пропущенные звонки");
			buttonMissingCallsReport.BindCommand(ViewModel.MissingCallsReportCommand);

			tableButtons.Add(buttonMissingCallsReport);
			Gtk.Table.TableChild tc1 = ((Gtk.Table.TableChild)(this.tableButtons[buttonMissingCallsReport]));
			tc1.TopAttach = 1;
			tc1.BottomAttach = 1;
			tc1.XOptions = Gtk.AttachOptions.Fill;
			tc1.YOptions = Gtk.AttachOptions.Fill;

			tableButtons.ShowAll();
		}
	}
}
