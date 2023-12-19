using Gtk;
using System;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;

namespace Vodovoz.Dialogs.Interactive
{
	public partial class ConfirmationQuestionDlg : Dialog
	{
		private readonly ConfirmationQuestionDialogSettings _dialogInfo;
		private readonly ConfirmationQuestion[] _questions;

		public ConfirmationQuestionDlg(ConfirmationQuestionDialogSettings dialogSettings, params ConfirmationQuestion[] questions)
		{
			_dialogInfo = dialogSettings ?? throw new ArgumentNullException(nameof(dialogSettings));
			_questions = questions ?? new ConfirmationQuestion[0];

			Build();
			Configure();
		}

		private void Configure()
		{
			Title = _dialogInfo.Title ?? string.Empty;

			if(!string.IsNullOrWhiteSpace(_dialogInfo.TopText))
			{
				ylabelTopText.Text = _dialogInfo.TopText;
				ylabelTopText.Visible = true;
			}

			if(!string.IsNullOrWhiteSpace(_dialogInfo.BottomText))
			{
				ylabelBottomText.Text = _dialogInfo.BottomText;
				ylabelBottomText.Visible = true;
			}

			ShowImage();

			UpdateButtonsSensitive();

			UpdateQuestions();
		}

		private void ShowImage()
		{
			switch(_dialogInfo.ImageType)
			{
				case (ConfirmationQuestionDialogSettings.ImgType.Question):
					imageQuestion.Visible = true;
					break;
				case ConfirmationQuestionDialogSettings.ImgType.Warning:
					imageWarning.Visible = true;
					break;
				case ConfirmationQuestionDialogSettings.ImgType.Error:
					imageError.Visible = true;
					break;
				case ConfirmationQuestionDialogSettings.ImgType.Info:
					imageInfo.Visible = true;
					break;
				default:
					throw new ArgumentException("Тип не известен");
			}
		}

		private void UpdateQuestions()
		{
			if(_questions.Length > 0)
			{
				yvboxQuestion1.Visible = true;
				ylabelQuestion1.Text = _questions[0].QuestionText;
				ycheckbuttonConfirmation1.Label = _questions[0].ConfirmationText;
				ycheckbuttonConfirmation1.Clicked += (s, e) => UpdateButtonsSensitive();
			}

			if(_questions.Length > 1)
			{
				yvboxQuestion2.Visible = true;
				ylabelQuestion2.Text = _questions[1].QuestionText;
				ycheckbuttonConfirmation2.Label = _questions[1].ConfirmationText;
				ycheckbuttonConfirmation2.Clicked += (s, e) => UpdateButtonsSensitive();
			}

			if(_questions.Length > 2)
			{
				yvboxQuestion3.Visible = true;
				ylabelQuestion3.Text = _questions[2].QuestionText;
				ycheckbuttonConfirmation3.Label = _questions[2].ConfirmationText;
				ycheckbuttonConfirmation3.Clicked += (s, e) => UpdateButtonsSensitive();
			}
		}

		private void UpdateButtonsSensitive()
		{
			bool isCheckbuttonsActivated =
				(_questions.Length < 1 || ycheckbuttonConfirmation1.Active)
				&& (_questions.Length < 2 || ycheckbuttonConfirmation2.Active)
				&& (_questions.Length < 3 || ycheckbuttonConfirmation3.Active);

			buttonYes.Sensitive = _dialogInfo.IsYesButtonAvailableByDefault || isCheckbuttonsActivated;

			buttonNo.Sensitive = _dialogInfo.IsNoButtonAvailableByDefault || isCheckbuttonsActivated;			
		}
	}
}
