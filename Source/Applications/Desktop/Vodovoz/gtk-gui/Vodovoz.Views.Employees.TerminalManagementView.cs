
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Employees
{
	public partial class TerminalManagementView
	{
		private global::Gamma.GtkWidgets.yHBox yhbox1;

		private global::Gamma.GtkWidgets.yButton buttonGiveout;

		private global::Gamma.GtkWidgets.yButton buttonReturn;

		private global::Gamma.GtkWidgets.yLabel labelTerminalInfo;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Employees.TerminalManagementView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Employees.TerminalManagementView";
			// Container child Vodovoz.Views.Employees.TerminalManagementView.Gtk.Container+ContainerChild
			this.yhbox1 = new global::Gamma.GtkWidgets.yHBox();
			this.yhbox1.Name = "yhbox1";
			this.yhbox1.Spacing = 6;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.buttonGiveout = new global::Gamma.GtkWidgets.yButton();
			this.buttonGiveout.CanFocus = true;
			this.buttonGiveout.Name = "buttonGiveout";
			this.buttonGiveout.UseUnderline = true;
			this.buttonGiveout.Label = global::Mono.Unix.Catalog.GetString("Привязать терминал");
			this.yhbox1.Add(this.buttonGiveout);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.buttonGiveout]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.buttonReturn = new global::Gamma.GtkWidgets.yButton();
			this.buttonReturn.CanFocus = true;
			this.buttonReturn.Name = "buttonReturn";
			this.buttonReturn.UseUnderline = true;
			this.buttonReturn.Label = global::Mono.Unix.Catalog.GetString("Отвязать терминал");
			this.yhbox1.Add(this.buttonReturn);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.buttonReturn]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child yhbox1.Gtk.Box+BoxChild
			this.labelTerminalInfo = new global::Gamma.GtkWidgets.yLabel();
			this.labelTerminalInfo.Name = "labelTerminalInfo";
			this.labelTerminalInfo.LabelProp = global::Mono.Unix.Catalog.GetString("Терминал не привязан");
			this.yhbox1.Add(this.labelTerminalInfo);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.yhbox1[this.labelTerminalInfo]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			this.Add(this.yhbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
