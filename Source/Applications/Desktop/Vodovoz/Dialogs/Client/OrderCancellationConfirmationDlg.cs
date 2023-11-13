using System;

namespace Vodovoz.Dialogs.Client
{
	public partial class OrderCancellationConfirmationDlg : Gtk.Dialog
	{
		public OrderCancellationConfirmationDlg()
		{
			Build();
			Configure();
		}
		private void Configure()
		{
			ycheckbuttonConfirmation.Clicked += OnCheckbuttonConfirmationClicked;
		}

		private void OnCheckbuttonConfirmationClicked(object sender, EventArgs e)
		{
			buttonOk.Sensitive = !ycheckbuttonConfirmation.Active;
			buttonCancel.Sensitive = ycheckbuttonConfirmation.Active;
		}
	}
}
