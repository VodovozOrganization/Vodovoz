
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Settings
{
	public partial class RoboatsSettingsView
	{
		private global::Gamma.GtkWidgets.yHBox yhboxSettings;

		private global::Gamma.GtkWidgets.yCheckButton ycheckbuttonRoboatsEnabled;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Settings.RoboatsSettingsView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Settings.RoboatsSettingsView";
			// Container child Vodovoz.Views.Settings.RoboatsSettingsView.Gtk.Container+ContainerChild
			this.yhboxSettings = new global::Gamma.GtkWidgets.yHBox();
			this.yhboxSettings.Name = "yhboxSettings";
			this.yhboxSettings.Spacing = 6;
			// Container child yhboxSettings.Gtk.Box+BoxChild
			this.ycheckbuttonRoboatsEnabled = new global::Gamma.GtkWidgets.yCheckButton();
			this.ycheckbuttonRoboatsEnabled.CanFocus = true;
			this.ycheckbuttonRoboatsEnabled.Name = "ycheckbuttonRoboatsEnabled";
			this.ycheckbuttonRoboatsEnabled.Label = global::Mono.Unix.Catalog.GetString("Служба Roboats включена");
			this.ycheckbuttonRoboatsEnabled.DrawIndicator = true;
			this.ycheckbuttonRoboatsEnabled.UseUnderline = true;
			this.yhboxSettings.Add(this.ycheckbuttonRoboatsEnabled);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.yhboxSettings[this.ycheckbuttonRoboatsEnabled]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			this.Add(this.yhboxSettings);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
