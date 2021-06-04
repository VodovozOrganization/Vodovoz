using System;
namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PrintTimeRouteInfoWnd : Gtk.Bin
	{
		public PrintTimeRouteInfoWnd(DateTime time, int RLId)
		{
			this.Build();
			numberRL.Text = $"№ МЛ:{RLId}";
			dateRL.Text = time.ToLongDateString();
			timeRL.Text = time.ToShortTimeString();
		}
	}
}
