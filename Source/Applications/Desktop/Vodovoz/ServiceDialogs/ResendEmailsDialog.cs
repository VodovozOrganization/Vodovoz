using System;
using Vodovoz.Additions;
using QSProjectsLib;

namespace Vodovoz.ServiceDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ResendEmailsDialog : QS.Dialog.Gtk.TdiTabBase
	{
		public ResendEmailsDialog()
		{
			Build();
			TabName = "Переотправка почты";
			datepicker1.Date = DateTime.Now;
			buttonSendErrorSendedEmails.Clicked += ButtonSendErrorSendedEmails_Clicked;
		}

		void ButtonSendErrorSendedEmails_Clicked(object sender, EventArgs e)
		{
			ManualEmailSender emailSender = new ManualEmailSender();
			emailSender.ResendEmailWithErrorSendingStatus(datepicker1.Date);
			MessageDialogWorks.RunInfoDialog("Done");
		}
	}
}
