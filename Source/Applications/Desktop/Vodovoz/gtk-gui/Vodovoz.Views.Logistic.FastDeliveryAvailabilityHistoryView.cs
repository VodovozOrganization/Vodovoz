
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Logistic
{
	public partial class FastDeliveryAvailabilityHistoryView
	{
		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yVBox vboxMain;

		private global::Gamma.GtkWidgets.yHBox yhboxMainButtons;

		private global::Gamma.GtkWidgets.yButton buttonClose;

		private global::Gamma.GtkWidgets.yHBox yhboxVerificationWidget;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Logistic.FastDeliveryAvailabilityHistoryView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Logistic.FastDeliveryAvailabilityHistoryView";
			// Container child Vodovoz.Views.Logistic.FastDeliveryAvailabilityHistoryView.Gtk.Container+ContainerChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.vboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.yhboxMainButtons = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxMainButtons.Name = "yhboxMainButtons";
			this.yhboxMainButtons.Spacing = 6;
			// Container child yhboxMainButtons.Gtk.Box+BoxChild
			this.buttonClose = new global::Gamma.GtkWidgets.yButton();
			this.buttonClose.CanFocus = true;
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.UseUnderline = true;
			this.buttonClose.Label = global::Mono.Unix.Catalog.GetString("Закрыть");
			global::Gtk.Image w1 = new global::Gtk.Image();
			w1.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-close", global::Gtk.IconSize.Menu);
			this.buttonClose.Image = w1;
			this.yhboxMainButtons.Add(this.buttonClose);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.yhboxMainButtons[this.buttonClose]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			this.vboxMain.Add(this.yhboxMainButtons);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.yhboxMainButtons]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.yhboxVerificationWidget = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxVerificationWidget.Name = "yhboxVerificationWidget";
			this.yhboxVerificationWidget.Spacing = 6;
			this.vboxMain.Add(this.yhboxVerificationWidget);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.yhboxVerificationWidget]));
			w4.Position = 1;
			this.yhbox1.Add(this.vboxMain);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.vboxMain]));
			w5.Position = 0;
			this.Add(this.yhbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
