
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.ViewWidgets
{
	public partial class DepositRefundItemsView
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hboxDeposit;

		private global::Gamma.GtkWidgets.yButton buttonNewBottleDeposit;

		private global::Gamma.GtkWidgets.yButton buttonNewEquipmentDeposit;

		private global::Gamma.GtkWidgets.yButton buttonDeleteDeposit;

		private global::Gtk.ScrolledWindow scrolledwindow2;

		private global::Gamma.GtkWidgets.yTreeView treeDepositRefundItems;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.ViewWidgets.DepositRefundItemsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.ViewWidgets.DepositRefundItemsView";
			// Container child Vodovoz.ViewWidgets.DepositRefundItemsView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hboxDeposit = new global::Gtk.HBox();
			this.hboxDeposit.Name = "hboxDeposit";
			this.hboxDeposit.Spacing = 6;
			// Container child hboxDeposit.Gtk.Box+BoxChild
			this.buttonNewBottleDeposit = new global::Gamma.GtkWidgets.yButton();
			this.buttonNewBottleDeposit.CanFocus = true;
			this.buttonNewBottleDeposit.Name = "buttonNewBottleDeposit";
			this.buttonNewBottleDeposit.UseUnderline = true;
			this.buttonNewBottleDeposit.Label = global::Mono.Unix.Catalog.GetString("Бутыли");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonNewBottleDeposit.Image = w1;
			this.hboxDeposit.Add(this.buttonNewBottleDeposit);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxDeposit[this.buttonNewBottleDeposit]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hboxDeposit.Gtk.Box+BoxChild
			this.buttonNewEquipmentDeposit = new global::Gamma.GtkWidgets.yButton();
			this.buttonNewEquipmentDeposit.CanFocus = true;
			this.buttonNewEquipmentDeposit.Name = "buttonNewEquipmentDeposit";
			this.buttonNewEquipmentDeposit.UseUnderline = true;
			this.buttonNewEquipmentDeposit.Label = global::Mono.Unix.Catalog.GetString("Оборудование");
			global::Gtk.Image w3 = new global::Gtk.Image();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonNewEquipmentDeposit.Image = w3;
			this.hboxDeposit.Add(this.buttonNewEquipmentDeposit);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxDeposit[this.buttonNewEquipmentDeposit]));
			w4.Position = 1;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hboxDeposit.Gtk.Box+BoxChild
			this.buttonDeleteDeposit = new global::Gamma.GtkWidgets.yButton();
			this.buttonDeleteDeposit.CanFocus = true;
			this.buttonDeleteDeposit.Name = "buttonDeleteDeposit";
			this.buttonDeleteDeposit.UseUnderline = true;
			this.buttonDeleteDeposit.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w5 = new global::Gtk.Image();
			w5.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			this.buttonDeleteDeposit.Image = w5;
			this.hboxDeposit.Add(this.buttonDeleteDeposit);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hboxDeposit[this.buttonDeleteDeposit]));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			this.vbox2.Add(this.hboxDeposit);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hboxDeposit]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.scrolledwindow2 = new global::Gtk.ScrolledWindow();
			this.scrolledwindow2.CanFocus = true;
			this.scrolledwindow2.Name = "scrolledwindow2";
			this.scrolledwindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow2.Gtk.Container+ContainerChild
			this.treeDepositRefundItems = new global::Gamma.GtkWidgets.yTreeView();
			this.treeDepositRefundItems.CanFocus = true;
			this.treeDepositRefundItems.Name = "treeDepositRefundItems";
			this.scrolledwindow2.Add(this.treeDepositRefundItems);
			this.vbox2.Add(this.scrolledwindow2);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.scrolledwindow2]));
			w9.Position = 1;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.hboxDeposit.Hide();
			this.treeDepositRefundItems.Hide();
			this.Hide();
			this.buttonNewBottleDeposit.Clicked += new global::System.EventHandler(this.OnButtonNewBottleDepositClicked);
			this.buttonNewEquipmentDeposit.Clicked += new global::System.EventHandler(this.OnButtonNewEquipmentDepositClicked);
			this.buttonDeleteDeposit.Clicked += new global::System.EventHandler(this.OnButtonDeleteDepositClicked);
		}
	}
}
