
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Filters.GtkViews
{
	public partial class ProductGroupJournalFilterView
	{
		private global::Gtk.HBox hbox1;

		private global::Gamma.GtkWidgets.yCheckButton ycheckArchive;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Filters.GtkViews.ProductGroupJournalFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Filters.GtkViews.ProductGroupJournalFilterView";
			// Container child Vodovoz.Filters.GtkViews.ProductGroupJournalFilterView.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.ycheckArchive = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckArchive.CanFocus = true;
			this.ycheckArchive.Name = "ycheckArchive";
			this.ycheckArchive.Label = global::Mono.Unix.Catalog.GetString("Скрыть архивированные");
			this.ycheckArchive.DrawIndicator = true;
			this.ycheckArchive.UseUnderline = true;
			this.hbox1.Add(this.ycheckArchive);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.ycheckArchive]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			this.Add(this.hbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
