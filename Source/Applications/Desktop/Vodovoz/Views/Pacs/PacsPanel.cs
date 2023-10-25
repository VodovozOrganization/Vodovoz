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

			SetFastButtonsIconSize(IconSize.Dnd);
		}

		public void SetIconSize(IconSize iconSize)
		{
			SetFastButtonsIconSize(iconSize);
		}

		private void SetFastButtonsIconSize(IconSize iconSize)
		{
			if(iconSize < IconSize.LargeToolbar)
			{
				iconSize = IconSize.LargeToolbar;
			}

			IconSize smallButtonsSize;

			switch(iconSize)
			{
				case IconSize.Dialog:
					smallButtonsSize = IconSize.LargeToolbar;
					break;
				case IconSize.Dnd:
				case IconSize.LargeToolbar:
					smallButtonsSize = IconSize.SmallToolbar;
					break;
				case IconSize.SmallToolbar:
				case IconSize.Menu:
				case IconSize.Button:
				case IconSize.Invalid:
				default:
					smallButtonsSize = IconSize.Menu;
					break;
			}

			Image imageButtonBreak = new Image();
			imageButtonBreak.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-allowed", smallButtonsSize);
			this.buttonBreak.Image = imageButtonBreak;

			Image imageButtonRefresh = new Image();
			imageButtonRefresh.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-refresh", smallButtonsSize);
			this.buttonRefresh.Image = imageButtonRefresh;

			Image imageButtonPacs = new Image();
			imageButtonPacs.Pixbuf = Stetic.IconLoader.LoadIcon(this, "pacs-active", iconSize);
			this.buttonPacs.Image = imageButtonPacs;

			Image imageButtonMango = new Image();
			imageButtonMango.Pixbuf = Stetic.IconLoader.LoadIcon(this, "phone-disable", iconSize);
			this.buttonMango.Image = imageButtonMango;
		}
	}


}
