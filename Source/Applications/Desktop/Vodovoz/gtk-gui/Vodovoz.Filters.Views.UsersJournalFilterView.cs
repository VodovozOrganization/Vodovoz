
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Filters.Views
{
	public partial class UsersJournalFilterView
	{
		private global::Gamma.GtkWidgets.yVBox vboxMain;

		private global::Gamma.GtkWidgets.yHBox hboxMain;

		private global::Gamma.GtkWidgets.yCheckButton chkShowDeactivatedUsers;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Filters.Views.UsersJournalFilterView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Filters.Views.UsersJournalFilterView";
			// Container child Vodovoz.Filters.Views.UsersJournalFilterView.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hboxMain = new global::Gamma.GtkWidgets.yHBox();
			this.hboxMain.Name = "hboxMain";
			this.hboxMain.Spacing = 6;
			// Container child hboxMain.Gtk.Box+BoxChild
			this.chkShowDeactivatedUsers = new global::Gamma.GtkWidgets.yCheckButton();
			this.chkShowDeactivatedUsers.CanFocus = true;
			this.chkShowDeactivatedUsers.Name = "chkShowDeactivatedUsers";
			this.chkShowDeactivatedUsers.Label = global::Mono.Unix.Catalog.GetString("Показывать деактивированных пользователей");
			this.chkShowDeactivatedUsers.DrawIndicator = true;
			this.chkShowDeactivatedUsers.UseUnderline = true;
			this.hboxMain.Add(this.chkShowDeactivatedUsers);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hboxMain[this.chkShowDeactivatedUsers]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			this.vboxMain.Add(this.hboxMain);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hboxMain]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			this.Add(this.vboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
