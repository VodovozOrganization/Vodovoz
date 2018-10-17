using System;
namespace Vodovoz.DriverTerminal
{
	public partial class DriverTerminalWindow : Gtk.Window
	{
		public DriverTerminalWindow() :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}
	}
}
