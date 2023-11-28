using System;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;

namespace Vodovoz.Dialogs.Interactive
{
	public partial class ConfirmationQuestionDlg : Gtk.Dialog
	{
		private readonly ConfirmationQuestionDialogSettings _dialogInfo;
		private readonly ConfirmationQuestion[] _questions;

		public ConfirmationQuestionDlg(ConfirmationQuestionDialogSettings dialogInfo, params ConfirmationQuestion[] questions)
		{
			_dialogInfo = dialogInfo ?? throw new ArgumentNullException(nameof(dialogInfo));

			_questions = questions ?? new ConfirmationQuestion[0];

			Build();
			Configure();
		}

		private void Configure()
		{
			ShowImage();

			Title = _dialogInfo.Title;

			//buttonOk.Sensitive = false;
			//buttonCancel.Sensitive = true;

			if(_questions.Length > 0)
			{
				yvboxQuestion1.Visible = true;
				ylabelQuestion1.Text = _questions[0].QuestionText;
				ycheckbuttonConfirmation1.Label = _questions[0].ConfirmationText;
				ycheckbuttonConfirmation1.Clicked += (s, e) => UpdateButtonCancelSensitive();
			}


			if(_questions.Length > 1)
			{
				yvboxQuestion2.Visible = true;
				ylabelQuestion2.Text = _questions[1].QuestionText;
				ycheckbuttonConfirmation2.Label = _questions[1].ConfirmationText;
				ycheckbuttonConfirmation2.Clicked += (s, e) => UpdateButtonCancelSensitive();
			}

			if(_questions.Length > 3)
			{
				yvboxQuestion3.Visible = true;
				ylabelQuestion3.Text = _questions[2].QuestionText;
				ycheckbuttonConfirmation3.Label = _questions[2].ConfirmationText;
				ycheckbuttonConfirmation3.Clicked += (s, e) => UpdateButtonCancelSensitive();
			}
		}

		private void ShowImage()
		{
			imageQuestion.Visible = true;
		}

		private void UpdateButtonCancelSensitive()
		{
			//buttonOk.Sensitive = true;
				//ycheckbuttonConfirmation1.Active
				//&& (_question2 == null || ycheckbuttonConfirmation2.Active)
				//&& (_question3 == null || ycheckbuttonConfirmation3.Active);
		}
	}
}
