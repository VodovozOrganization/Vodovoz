using System;
using Vodovoz.Domain.Contacts;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewWidgets.Mango
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class HandsetView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private Phone Phone;

		public HandsetView(string number)
		{
			this.Build();
			Phone = new Phone();
			Phone.Number = number;
		}

		protected void Clicked_buttonMakeCall(object sender, EventArgs e)
		{
			MainClass.MainWin.MangoManager.MakeCall("7"+Phone.DigitsNumber);
		}
	}
}
