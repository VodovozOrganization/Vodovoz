using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Dialogs.Interactive;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;
using static Vodovoz.Tools.Interactive.ConfirmationQuestion.ConfirmationQuestionDialogSettings;

namespace Vodovoz.Infrastructure.Services
{
	public class GtkConfirmationQuestionInteractive : IConfirmationQuestionInteractive
	{
		public bool Question(
			IEnumerable<ConfirmationQuestion> questions,
			string title = null,
			string topText = null,
			string bottomText = null,
			bool isYesButtonAvailableByDefault = false,
			bool isNoButtonAvailableByDefault = false,
			ImgType imageType = ImgType.Question)
		{
			if(questions == null)
			{
				throw new ArgumentNullException(nameof(questions));
			}

			var dialogSettings = new ConfirmationQuestionDialogSettings
			{
				Title = title ?? string.Empty,
				TopText = topText ?? string.Empty,
				BottomText = bottomText ?? string.Empty,
				IsYesButtonAvailableByDefault = isYesButtonAvailableByDefault,
				IsNoButtonAvailableByDefault = isNoButtonAvailableByDefault,
				ImageType = imageType
			};

			var dlg = new ConfirmationQuestionDlg(dialogSettings, questions.ToArray());
			dlg.SetPosition(WindowPosition.CenterAlways);
			var result = dlg.Run() == (int)ResponseType.Yes;
			dlg.Destroy();

			return result;
		}
	}
}
