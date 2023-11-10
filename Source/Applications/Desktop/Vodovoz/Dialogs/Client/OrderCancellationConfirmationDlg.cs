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
			ycheckbuttonConfirmation.StateChanged += OnCheckbuttonConfirmationStateChanged;
		}

		private void OnCheckbuttonConfirmationStateChanged(object o, Gtk.StateChangedArgs args)
		{
			buttonOk.Sensitive = ycheckbuttonConfirmation.Active;
			buttonCancel.Sensitive = !ycheckbuttonConfirmation.Active;
		}
	}
}
