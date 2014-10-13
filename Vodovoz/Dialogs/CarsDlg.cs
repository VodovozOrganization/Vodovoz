using System;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CarsDlg : Gtk.Bin
	{
		public CarsDlg ()
		{
			this.Build ();
		}

		protected void OnRadiobuttonFilesToggled (object sender, EventArgs e)
		{
			if (radiobuttonFiles.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadiobuttonMainToggled (object sender, EventArgs e)
		{
			if (radiobuttonFiles.Active)
				notebook1.CurrentPage = 0;
		}
	}
}

