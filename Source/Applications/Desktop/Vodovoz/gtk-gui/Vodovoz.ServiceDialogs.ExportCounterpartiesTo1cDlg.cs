
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ServiceDialogs
{
	public partial class ExportCounterpartiesTo1cDlg
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox3;

		private global::Gamma.GtkWidgets.yButton btnRunToFile;

		private global::Gtk.ScrolledWindow GtkScrolledWindowErrors;

		private global::Gtk.TextView textviewErrors;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ServiceDialogs.ExportCounterpartiesTo1cDlg
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ServiceDialogs.ExportCounterpartiesTo1cDlg";
			// Container child Vodovoz.ServiceDialogs.ExportCounterpartiesTo1cDlg.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.btnRunToFile = new global::Gamma.GtkWidgets.yButton();
			this.btnRunToFile.CanFocus = true;
			this.btnRunToFile.Name = "btnRunToFile";
			this.btnRunToFile.UseUnderline = true;
			this.btnRunToFile.Label = global::Mono.Unix.Catalog.GetString("Экспортировать в файл");
			this.hbox3.Add(this.btnRunToFile);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox3[this.btnRunToFile]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			this.vbox1.Add(this.hbox3);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox3]));
			w2.Position = 0;
			w2.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindowErrors = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindowErrors.Name = "GtkScrolledWindowErrors";
			this.GtkScrolledWindowErrors.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindowErrors.Gtk.Container+ContainerChild
			this.textviewErrors = new global::Gtk.TextView();
			this.textviewErrors.CanFocus = true;
			this.textviewErrors.Name = "textviewErrors";
			this.textviewErrors.Editable = false;
			this.GtkScrolledWindowErrors.Add(this.textviewErrors);
			this.vbox1.Add(this.GtkScrolledWindowErrors);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindowErrors]));
			w4.Position = 1;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.GtkScrolledWindowErrors.Hide();
			this.Hide();
			this.btnRunToFile.Clicked += new global::System.EventHandler(this.OnBtnRunToFileClicked);
		}
	}
}
