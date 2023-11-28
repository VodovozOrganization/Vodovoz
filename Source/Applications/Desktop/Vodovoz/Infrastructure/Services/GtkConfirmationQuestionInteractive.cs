using Gtk;
using Vodovoz.Dialogs.Interactive;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;

namespace Vodovoz.Infrastructure.Services
{
	public class GtkConfirmationQuestionInteractive : IConfirmationQuestionInteractive
	{
		public bool Question(ConfirmationQuestionDialogInfo dialogInfo, params ConfirmationQuestion[] questions)
		{
			var dlg = new ConfirmationQuestionDlg(dialogInfo, questions);
			dlg.SetPosition(WindowPosition.CenterAlways);
			var result = dlg.Run() == (int)ResponseType.Yes;
			dlg.Destroy();

			return result;
		}
	}
}
