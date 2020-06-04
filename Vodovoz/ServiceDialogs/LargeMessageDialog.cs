using System;
namespace Vodovoz.ServiceDialogs
{
	public partial class LargeMessageDialog : Gtk.Window
	{
		public LargeMessageDialog(string title, string message) :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			Modal = true;
			Title = title;
			ytextviewMessage.Buffer.Text = message;
			ybuttonOk.Clicked += (sender, e) => this.Destroy();
		}
	}
}
