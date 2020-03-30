using System;
namespace Vodovoz.Views.Search
{
	public partial class SolrSearchHelpWindow : Gtk.Window
	{
		public SolrSearchHelpWindow() :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			buttonClose.Clicked += (sender, e) => this.Destroy();
		}
	}
}
