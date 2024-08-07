
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ViewWidgets.Logistics
{
	public partial class AdditionalLoadingItemsView
	{
		private global::Gtk.VBox vbox4;

		private global::Gtk.HBox hbox4;

		private global::Gamma.GtkWidgets.yLabel ylabelAdditionalLoading;

		private global::Gamma.GtkWidgets.yButton ybuttonAdd;

		private global::Gamma.GtkWidgets.yButton ybuttonRemove;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeNomenclatures;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ViewWidgets.Logistics.AdditionalLoadingItemsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ViewWidgets.Logistics.AdditionalLoadingItemsView";
			// Container child Vodovoz.ViewWidgets.Logistics.AdditionalLoadingItemsView.Gtk.Container+ContainerChild
			this.vbox4 = new global::Gtk.VBox();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox();
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.ylabelAdditionalLoading = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelAdditionalLoading.Name = "ylabelAdditionalLoading";
			this.ylabelAdditionalLoading.Xalign = 0F;
			this.ylabelAdditionalLoading.LabelProp = global::Mono.Unix.Catalog.GetString("<b>ТМЦ в запасе:</b>");
			this.ylabelAdditionalLoading.UseMarkup = true;
			this.hbox4.Add(this.ylabelAdditionalLoading);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.ylabelAdditionalLoading]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.ybuttonAdd = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonAdd.CanFocus = true;
			this.ybuttonAdd.Name = "ybuttonAdd";
			this.ybuttonAdd.UseUnderline = true;
			this.ybuttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w2 = new global::Gtk.Image();
			w2.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.ybuttonAdd.Image = w2;
			this.hbox4.Add(this.ybuttonAdd);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.ybuttonAdd]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.ybuttonRemove = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonRemove.CanFocus = true;
			this.ybuttonRemove.Name = "ybuttonRemove";
			this.ybuttonRemove.UseUnderline = true;
			this.ybuttonRemove.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w4 = new global::Gtk.Image();
			w4.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.ybuttonRemove.Image = w4;
			this.hbox4.Add(this.ybuttonRemove);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox4[this.ybuttonRemove]));
			w5.Position = 2;
			w5.Expand = false;
			w5.Fill = false;
			this.vbox4.Add(this.hbox4);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.hbox4]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeNomenclatures = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeNomenclatures.CanFocus = true;
			this.ytreeNomenclatures.Name = "ytreeNomenclatures";
			this.GtkScrolledWindow.Add(this.ytreeNomenclatures);
			this.vbox4.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox4[this.GtkScrolledWindow]));
			w8.Position = 1;
			this.Add(this.vbox4);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
