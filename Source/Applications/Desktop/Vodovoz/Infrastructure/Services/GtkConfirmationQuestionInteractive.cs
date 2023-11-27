using Gtk;
using Vodovoz.Dialogs.Interactive;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;

namespace Vodovoz.Infrastructure.Services
{
	public class GtkConfirmationQuestionInteractive : IConfirmationQuestionInteractive
	{
		public bool Question(ConfirmationQuestionDialogInfo dialogInfo, ConfirmationQuestion question1, ConfirmationQuestion question2 = null, ConfirmationQuestion question3 = null)
		{
			var dlg = new ConfirmationQuestionDlg(dialogInfo, question1, question2, question3);
			dlg.SetPosition(WindowPosition.CenterAlways);
			var result = dlg.Run() == (int)ResponseType.Yes;
			dlg.Destroy();

			return result;
		}
	}
}
