
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ViewWidgets.GtkUI
{
	public partial class EntitySelection
	{
		private global::Gamma.GtkWidgets.yHBox yhboxMain;

		private global::Gamma.GtkWidgets.yEntry yentryObject;

		private global::Gamma.GtkWidgets.yButton ybuttonClear;

		private global::Gamma.GtkWidgets.yButton ybuttonSelectEntity;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ViewWidgets.GtkUI.EntitySelection
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ViewWidgets.GtkUI.EntitySelection";
			// Container child Vodovoz.ViewWidgets.GtkUI.EntitySelection.Gtk.Container+ContainerChild
			this.yhboxMain = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxMain.Name = "yhboxMain";
			this.yhboxMain.Spacing = 6;
			// Container child yhboxMain.Gtk.Box+BoxChild
			this.yentryObject = new global::Gamma.GtkWidgets.yEntry();
			this.yentryObject.CanFocus = true;
			this.yentryObject.Name = "yentryObject";
			this.yentryObject.IsEditable = true;
			this.yentryObject.InvisibleChar = '•';
			this.yhboxMain.Add(this.yentryObject);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.yhboxMain[this.yentryObject]));
			w1.Position = 0;
			// Container child yhboxMain.Gtk.Box+BoxChild
			this.ybuttonClear = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonClear.CanFocus = true;
			this.ybuttonClear.Name = "ybuttonClear";
			this.ybuttonClear.UseUnderline = true;
			global::Gtk.Image w2 = new global::Gtk.Image();
			w2.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-clear", global::Gtk.IconSize.Menu);
			this.ybuttonClear.Image = w2;
			this.yhboxMain.Add(this.ybuttonClear);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.yhboxMain[this.ybuttonClear]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child yhboxMain.Gtk.Box+BoxChild
			this.ybuttonSelectEntity = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonSelectEntity.CanFocus = true;
			this.ybuttonSelectEntity.Name = "ybuttonSelectEntity";
			this.ybuttonSelectEntity.UseUnderline = true;
			global::Gtk.Image w4 = new global::Gtk.Image();
			w4.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-index", global::Gtk.IconSize.Menu);
			this.ybuttonSelectEntity.Image = w4;
			this.yhboxMain.Add(this.ybuttonSelectEntity);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.yhboxMain[this.ybuttonSelectEntity]));
			w5.Position = 2;
			w5.Expand = false;
			w5.Fill = false;
			this.Add(this.yhboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
