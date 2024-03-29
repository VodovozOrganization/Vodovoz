
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ViewWidgets.Search
{
	public partial class CompositeSearchView
	{
		private global::Gtk.HBox hboxSearch;

		private global::Gamma.GtkWidgets.yEntry entrySearch1;

		private global::Gamma.GtkWidgets.yLabel ylabelSearchAnd;

		private global::Gamma.GtkWidgets.yEntry entrySearch2;

		private global::Gamma.GtkWidgets.yLabel ylabelSearchAnd2;

		private global::Gamma.GtkWidgets.yEntry entrySearch3;

		private global::Gamma.GtkWidgets.yLabel ylabelSearchAnd3;

		private global::Gamma.GtkWidgets.yEntry entrySearch4;

		private global::Gamma.GtkWidgets.yButton buttonAddAnd;

		private global::Gamma.GtkWidgets.yButton buttonSearchClear;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ViewWidgets.Search.CompositeSearchView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ViewWidgets.Search.CompositeSearchView";
			// Container child Vodovoz.ViewWidgets.Search.CompositeSearchView.Gtk.Container+ContainerChild
			this.hboxSearch = new global::Gtk.HBox();
			this.hboxSearch.Name = "hboxSearch";
			this.hboxSearch.Spacing = 6;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.entrySearch1 = new global::Gamma.GtkWidgets.yEntry();
			this.entrySearch1.WidthRequest = 50;
			this.entrySearch1.CanFocus = true;
			this.entrySearch1.Name = "entrySearch1";
			this.entrySearch1.IsEditable = true;
			this.entrySearch1.InvisibleChar = '•';
			this.hboxSearch.Add(this.entrySearch1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.entrySearch1]));
			w1.Position = 0;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.ylabelSearchAnd = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSearchAnd.Name = "ylabelSearchAnd";
			this.ylabelSearchAnd.LabelProp = global::Mono.Unix.Catalog.GetString("и");
			this.hboxSearch.Add(this.ylabelSearchAnd);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.ylabelSearchAnd]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.entrySearch2 = new global::Gamma.GtkWidgets.yEntry();
			this.entrySearch2.WidthRequest = 50;
			this.entrySearch2.CanFocus = true;
			this.entrySearch2.Name = "entrySearch2";
			this.entrySearch2.IsEditable = true;
			this.entrySearch2.InvisibleChar = '•';
			this.hboxSearch.Add(this.entrySearch2);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.entrySearch2]));
			w3.Position = 2;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.ylabelSearchAnd2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSearchAnd2.Name = "ylabelSearchAnd2";
			this.ylabelSearchAnd2.LabelProp = global::Mono.Unix.Catalog.GetString("и");
			this.hboxSearch.Add(this.ylabelSearchAnd2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.ylabelSearchAnd2]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.entrySearch3 = new global::Gamma.GtkWidgets.yEntry();
			this.entrySearch3.WidthRequest = 50;
			this.entrySearch3.CanFocus = true;
			this.entrySearch3.Name = "entrySearch3";
			this.entrySearch3.IsEditable = true;
			this.entrySearch3.InvisibleChar = '•';
			this.hboxSearch.Add(this.entrySearch3);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.entrySearch3]));
			w5.Position = 4;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.ylabelSearchAnd3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelSearchAnd3.Name = "ylabelSearchAnd3";
			this.ylabelSearchAnd3.LabelProp = global::Mono.Unix.Catalog.GetString("и");
			this.hboxSearch.Add(this.ylabelSearchAnd3);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.ylabelSearchAnd3]));
			w6.Position = 5;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.entrySearch4 = new global::Gamma.GtkWidgets.yEntry();
			this.entrySearch4.WidthRequest = 50;
			this.entrySearch4.CanFocus = true;
			this.entrySearch4.Name = "entrySearch4";
			this.entrySearch4.IsEditable = true;
			this.entrySearch4.InvisibleChar = '•';
			this.hboxSearch.Add(this.entrySearch4);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.entrySearch4]));
			w7.Position = 6;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.buttonAddAnd = new global::Gamma.GtkWidgets.yButton();
			this.buttonAddAnd.CanFocus = true;
			this.buttonAddAnd.Name = "buttonAddAnd";
			this.buttonAddAnd.UseUnderline = true;
			this.buttonAddAnd.Label = global::Mono.Unix.Catalog.GetString("И");
			global::Gtk.Image w8 = new global::Gtk.Image();
			w8.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAddAnd.Image = w8;
			this.hboxSearch.Add(this.buttonAddAnd);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.buttonAddAnd]));
			w9.Position = 7;
			w9.Expand = false;
			w9.Fill = false;
			// Container child hboxSearch.Gtk.Box+BoxChild
			this.buttonSearchClear = new global::Gamma.GtkWidgets.yButton();
			this.buttonSearchClear.TooltipMarkup = "Очистить";
			this.buttonSearchClear.CanFocus = true;
			this.buttonSearchClear.Name = "buttonSearchClear";
			this.buttonSearchClear.UseUnderline = true;
			global::Gtk.Image w10 = new global::Gtk.Image();
			w10.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-clear", global::Gtk.IconSize.Menu);
			this.buttonSearchClear.Image = w10;
			this.hboxSearch.Add(this.buttonSearchClear);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hboxSearch[this.buttonSearchClear]));
			w11.Position = 8;
			w11.Expand = false;
			w11.Fill = false;
			this.Add(this.hboxSearch);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.ylabelSearchAnd.Hide();
			this.entrySearch2.Hide();
			this.ylabelSearchAnd2.Hide();
			this.entrySearch3.Hide();
			this.ylabelSearchAnd3.Hide();
			this.entrySearch4.Hide();
			this.Hide();
		}
	}
}
