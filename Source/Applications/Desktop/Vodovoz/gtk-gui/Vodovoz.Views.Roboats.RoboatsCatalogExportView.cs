
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Roboats
{
	public partial class RoboatsCatalogExportView
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox1;

		private global::Gamma.Widgets.yEnumComboBox comboCatalog;

		private global::Gamma.GtkWidgets.yButton buttonExport;

		private global::Gtk.HSeparator hseparator1;

		private global::Gtk.HBox hbox2;

		private global::Gtk.VBox journalHolder;

		private global::Gtk.VBox dialogHolder;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Roboats.RoboatsCatalogExportView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Roboats.RoboatsCatalogExportView";
			// Container child Vodovoz.Views.Roboats.RoboatsCatalogExportView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.comboCatalog = new global::Gamma.Widgets.yEnumComboBox();
			this.comboCatalog.Name = "comboCatalog";
			this.comboCatalog.ShowSpecialStateAll = false;
			this.comboCatalog.ShowSpecialStateNot = false;
			this.comboCatalog.UseShortTitle = false;
			this.comboCatalog.DefaultFirst = false;
			this.hbox1.Add(this.comboCatalog);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.comboCatalog]));
			w1.Position = 0;
			w1.Expand = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonExport = new global::Gamma.GtkWidgets.yButton();
			this.buttonExport.CanFocus = true;
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.UseUnderline = true;
			this.buttonExport.Label = global::Mono.Unix.Catalog.GetString("Выгрузить");
			this.hbox1.Add(this.buttonExport);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonExport]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			this.vbox2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vbox2.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hseparator1]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Homogeneous = true;
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.journalHolder = new global::Gtk.VBox();
			this.journalHolder.Name = "journalHolder";
			this.journalHolder.Spacing = 6;
			this.hbox2.Add(this.journalHolder);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.journalHolder]));
			w5.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.dialogHolder = new global::Gtk.VBox();
			this.dialogHolder.Name = "dialogHolder";
			this.dialogHolder.Spacing = 6;
			this.hbox2.Add(this.dialogHolder);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.dialogHolder]));
			w6.Position = 1;
			this.vbox2.Add(this.hbox2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox2]));
			w7.Position = 2;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
