﻿
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.JournalViewers
{
	public partial class ProxyDocumentsView
	{
		private global::Gtk.VBox vbox1;

		private global::Gtk.HBox hbox1;

		private global::QS.Widgets.EnumMenuButton buttonAdd;

		private global::Gamma.GtkWidgets.yButton buttonEdit;

		private global::Gamma.GtkWidgets.yButton buttonDelete;

		private global::Gtk.CheckButton buttonFilter;

		private global::Gamma.GtkWidgets.yButton buttonRefresh;

		private global::Gtk.HBox hboxFilter;

		private global::QSWidgetLib.SearchEntity searchentity1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::QSOrmProject.RepresentationTreeView tableDocuments;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.JournalViewers.ProxyDocumentsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.JournalViewers.ProxyDocumentsView";
			// Container child Vodovoz.JournalViewers.ProxyDocumentsView.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonAdd = new global::QS.Widgets.EnumMenuButton();
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.UseMarkup = false;
			this.buttonAdd.LabelXAlign = 0F;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w1;
			this.hbox1.Add(this.buttonAdd);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonAdd]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonEdit = new global::Gamma.GtkWidgets.yButton();
			this.buttonEdit.CanFocus = true;
			this.buttonEdit.Name = "buttonEdit";
			this.buttonEdit.UseUnderline = true;
			this.buttonEdit.Label = global::Mono.Unix.Catalog.GetString("Изменить");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-edit", global::Gtk.IconSize.Menu);
			this.buttonEdit.Image = w3;
			this.hbox1.Add(this.buttonEdit);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonEdit]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonDelete = new global::Gamma.GtkWidgets.yButton();
			this.buttonDelete.CanFocus = true;
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.UseUnderline = true;
			this.buttonDelete.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.buttonDelete.Image = w5;
			this.hbox1.Add(this.buttonDelete);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonDelete]));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonFilter = new global::Gtk.CheckButton();
			this.buttonFilter.CanFocus = true;
			this.buttonFilter.Name = "buttonFilter";
			this.buttonFilter.Label = global::Mono.Unix.Catalog.GetString("Фильтр");
			this.buttonFilter.DrawIndicator = false;
			this.buttonFilter.UseUnderline = true;
			this.hbox1.Add(this.buttonFilter);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonFilter]));
			w7.PackType = ((global::Gtk.PackType)(1));
			w7.Position = 3;
			w7.Expand = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonRefresh = new global::Gamma.GtkWidgets.yButton();
			this.buttonRefresh.CanFocus = true;
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.UseUnderline = true;
			this.buttonRefresh.Label = global::Mono.Unix.Catalog.GetString("Обновить");
			global::Gtk.Image w8 = new global::Gtk.Image();
			w8.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-refresh", global::Gtk.IconSize.Menu);
			this.buttonRefresh.Image = w8;
			this.hbox1.Add(this.buttonRefresh);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonRefresh]));
			w9.PackType = ((global::Gtk.PackType)(1));
			w9.Position = 4;
			w9.Expand = false;
			w9.Fill = false;
			this.vbox1.Add(this.hbox1);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hbox1]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.hboxFilter = new global::Gtk.HBox();
			this.hboxFilter.Name = "hboxFilter";
			this.hboxFilter.Spacing = 6;
			this.vbox1.Add(this.hboxFilter);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.hboxFilter]));
			w11.Position = 1;
			w11.Expand = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.searchentity1 = new global::QSWidgetLib.SearchEntity();
			this.searchentity1.Events = ((global::Gdk.EventMask)(256));
			this.searchentity1.Name = "searchentity1";
			this.vbox1.Add(this.searchentity1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.searchentity1]));
			w12.Position = 2;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.tableDocuments = new global::QSOrmProject.RepresentationTreeView();
			this.tableDocuments.CanFocus = true;
			this.tableDocuments.Name = "tableDocuments";
			this.GtkScrolledWindow1.Add(this.tableDocuments);
			this.vbox1.Add(this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow1]));
			w14.Position = 3;
			this.Add(this.vbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.hboxFilter.Hide();
			this.Hide();
			this.buttonAdd.EnumItemClicked += new global::System.EventHandler<QS.Widgets.EnumItemClickedEventArgs>(this.OnButtonAddEnumItemClicked);
			this.buttonEdit.Clicked += new global::System.EventHandler(this.OnButtonEditClicked);
			this.buttonDelete.Clicked += new global::System.EventHandler(this.OnButtonDeleteClicked);
			this.buttonRefresh.Clicked += new global::System.EventHandler(this.OnButtonRefreshClicked);
			this.buttonFilter.Toggled += new global::System.EventHandler(this.OnButtonFilterToggled);
			this.searchentity1.TextChanged += new global::System.EventHandler(this.OnSearchentity1TextChanged);
			this.tableDocuments.RowActivated += new global::Gtk.RowActivatedHandler(this.OnTableDocumentsRowActivated);
		}
	}
}
