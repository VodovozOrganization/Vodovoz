using System;
namespace Vodovoz.Views.Mango.Incoming
{
	public partial class IncomingView : Gtk.Bin
	{
		public IncomingView()
		{
			this.Build();
		}

		public string Number { set { labelNumber.Markup = $"<b>{value}</b>"; } }
		public TimeSpan Time { set { labelTime.Text = value.ToString("mm\\:ss"); } }
		public string CallerName { set { labelName.Markup = value; } }
	}
}
