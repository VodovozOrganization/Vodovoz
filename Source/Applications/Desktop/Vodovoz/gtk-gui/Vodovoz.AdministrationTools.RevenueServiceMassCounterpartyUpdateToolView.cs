
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.AdministrationTools
{
	public partial class RevenueServiceMassCounterpartyUpdateToolView
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yLabel ylabelDate;

		private global::QS.Widgets.GtkUI.DateRangePicker datePeriodPicker;

		private global::Gamma.GtkWidgets.yButton ybuttonSearch;

		private global::Gamma.GtkWidgets.yButton ybuttonUpdateCounterparties;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView ytreeview1;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.AdministrationTools.RevenueServiceMassCounterpartyUpdateToolView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.AdministrationTools.RevenueServiceMassCounterpartyUpdateToolView";
			// Container child Vodovoz.AdministrationTools.RevenueServiceMassCounterpartyUpdateToolView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ylabelDate = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelDate.Name = "ylabelDate";
			this.ylabelDate.LabelProp = global::Mono.Unix.Catalog.GetString("Интервал последних продаж:");
			this.hbox1.Add(this.ylabelDate);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ylabelDate]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.datePeriodPicker = new global::QS.Widgets.GtkUI.DateRangePicker();
			this.datePeriodPicker.Events = ((global::Gdk.EventMask)(256));
			this.datePeriodPicker.Name = "datePeriodPicker";
			this.datePeriodPicker.StartDate = new global::System.DateTime(0);
			this.datePeriodPicker.EndDate = new global::System.DateTime(0);
			this.hbox1.Add(this.datePeriodPicker);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.datePeriodPicker]));
			w2.Position = 1;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ybuttonSearch = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSearch.CanFocus = true;
			this.ybuttonSearch.Name = "ybuttonSearch";
			this.ybuttonSearch.UseUnderline = true;
			this.ybuttonSearch.Label = global::Mono.Unix.Catalog.GetString("Искать");
			this.hbox1.Add(this.ybuttonSearch);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ybuttonSearch]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ybuttonUpdateCounterparties = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonUpdateCounterparties.CanFocus = true;
			this.ybuttonUpdateCounterparties.Name = "ybuttonUpdateCounterparties";
			this.ybuttonUpdateCounterparties.UseUnderline = true;
			this.ybuttonUpdateCounterparties.Label = global::Mono.Unix.Catalog.GetString("Обновить контрагентов");
			this.hbox1.Add(this.ybuttonUpdateCounterparties);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ybuttonUpdateCounterparties]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			this.vbox2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.ytreeview1 = new global::Gamma.GtkWidgets.yTreeView();
			this.ytreeview1.CanFocus = true;
			this.ytreeview1.Name = "ytreeview1";
			this.GtkScrolledWindow.Add(this.ytreeview1);
			this.vbox2.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.GtkScrolledWindow]));
			w7.Position = 1;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
