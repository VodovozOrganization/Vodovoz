using System;
using QSOrmProject;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		public CounterpartyDlg ()
		{
			this.Build ();
		}
	}
}

