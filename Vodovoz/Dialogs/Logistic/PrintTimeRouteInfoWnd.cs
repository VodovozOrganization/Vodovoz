using System;
namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PrintTimeRouteInfoWnd : Gtk.Window
	{
		public PrintTimeRouteInfoWnd(DateTime? time, int RLId) : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			numberRL.Text = $"№ МЛ:{RLId}";
			if(time.HasValue)
			{
				dateRL.Text = $"Дата печати {time?.ToLongDateString()}";
				timeRL.Text = $"Время печати {time?.ToShortTimeString()}";
			}
			else
			{
				dateRL.Text = $"МЛ:{RLId} не был распечатан";
			}
		}
	}
}
