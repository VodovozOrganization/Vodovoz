using QS.Dialog;
using System;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;

namespace Vodovoz.Dialogs.Interactive
{
	public partial class ConfirmationQuestionDlg : Gtk.Dialog
	{
		private readonly ConfirmationQuestionDialogInfo _dialogInfo;
		private readonly ConfirmationQuestion _question1;
		private readonly ConfirmationQuestion _question2;
		private readonly ConfirmationQuestion _question3;

		public ConfirmationQuestionDlg(
			ConfirmationQuestionDialogInfo dialogInfo,
			ConfirmationQuestion question1,
			ConfirmationQuestion question2 = null,
			ConfirmationQuestion question3 = null)
		{
			Build();
			Configure();
			_dialogInfo = dialogInfo ?? throw new ArgumentNullException(nameof(dialogInfo));
			_question1 = question1 ?? throw new ArgumentNullException(nameof(question1));
			_question2 = question2;
			_question3 = question3;
		}

		private void Configure()
		{
			ShowImage();

			Title = _dialogInfo.Title;

			yvboxQuestion1.Visible = true;
			ylabelQuestion1.Text = _question1.QuestionText;
			ycheckbuttonConfirmation1.TooltipText = _question1.ConfirmationText;
			ycheckbuttonConfirmation1.Clicked += (s, e) => UpdateButtonCancelSensitive();

			if( _question2 != null )
			{
				yvboxQuestion2.Visible = true;
				ylabelQuestion2.Text = _question2.QuestionText;
				ycheckbuttonConfirmation2.TooltipText = _question2.ConfirmationText;
				ycheckbuttonConfirmation2.Clicked += (s, e) => UpdateButtonCancelSensitive();
			}

			if(_question3 != null)
			{
				yvboxQuestion3.Visible = true;
				ylabelQuestion3.Text = _question3.QuestionText;
				ycheckbuttonConfirmation3.TooltipText = _question3.ConfirmationText;
				ycheckbuttonConfirmation3.Clicked += (s, e) => UpdateButtonCancelSensitive();
			}
		}

		private void ShowImage()
		{
			imageQuestion.Visible = true;
		}

		private void UpdateButtonCancelSensitive()
		{
			buttonCancel.Sensitive =
				ycheckbuttonConfirmation1.Active
				&& (_question2 == null || ycheckbuttonConfirmation2.Active)
				&& (_question3 == null || ycheckbuttonConfirmation3.Active);
		}
	}
}
