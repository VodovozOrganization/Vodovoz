
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.JournalViewers
{
	public partial class OrderSelectedView
	{
		private global::Gtk.VBox vbox3;

		private global::Gtk.Label label1;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Label label3;

		private global::Gamma.Widgets.yValidatedEntry yvalidatedentry1;

		private global::Gtk.Label label2;

		private global::QS.Views.Control.EntityEntry entryCounterparty;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::QSOrmProject.RepresentationTreeView datatreeviewOrderDocuments;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.JournalViewers.OrderSelectedView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.JournalViewers.OrderSelectedView";
			// Container child Vodovoz.JournalViewers.OrderSelectedView.Gtk.Container+ContainerChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			this.vbox3.BorderWidth = ((uint)(6));
			// Container child vbox3.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Документы заказа:");
			this.vbox3.Add(this.label1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.label1]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Заказ:");
			this.hbox1.Add(this.label3);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.label3]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.yvalidatedentry1 = new global::Gamma.Widgets.yValidatedEntry();
			this.yvalidatedentry1.CanFocus = true;
			this.yvalidatedentry1.Name = "yvalidatedentry1";
			this.yvalidatedentry1.IsEditable = true;
			this.yvalidatedentry1.InvisibleChar = '•';
			this.hbox1.Add(this.yvalidatedentry1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.yvalidatedentry1]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Контрагент:");
			this.hbox1.Add(this.label2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.label2]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.entryCounterparty = new global::QS.Views.Control.EntityEntry();
			this.entryCounterparty.Events = ((global::Gdk.EventMask)(256));
			this.entryCounterparty.Name = "entryCounterparty";
			this.hbox1.Add(this.entryCounterparty);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.entryCounterparty]));
			w5.Position = 3;
			this.vbox3.Add(this.hbox1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.hbox1]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.datatreeviewOrderDocuments = new global::QSOrmProject.RepresentationTreeView();
			this.datatreeviewOrderDocuments.CanFocus = true;
			this.datatreeviewOrderDocuments.Name = "datatreeviewOrderDocuments";
			this.GtkScrolledWindow.Add(this.datatreeviewOrderDocuments);
			this.vbox3.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.GtkScrolledWindow]));
			w8.Position = 2;
			this.Add(this.vbox3);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.yvalidatedentry1.Changed += new global::System.EventHandler(this.OnYvalidatedentry1Changed);
			this.datatreeviewOrderDocuments.RowActivated += new global::Gtk.RowActivatedHandler(this.OnDatatreeviewOrderDocumentsRowActivated);
		}
	}
}
