using System;
using Gtk;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsPanel : Gtk.Bin
	{
		public PacsPanel()
		{
			this.Build();


		}

		public IconSize IconSize
		{
			get => default(IconSize);
			set
			{
			}
		}

		private void SetFastButtonsIconSize(IconSize iconSize)
		{
			Image image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-refresh", IconSize.Dialog);
			this.buttonBreak.Image = image;
		}
	}


}
