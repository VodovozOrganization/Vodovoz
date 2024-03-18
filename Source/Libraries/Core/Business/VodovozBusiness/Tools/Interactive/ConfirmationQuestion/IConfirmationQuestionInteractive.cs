using System.Collections.Generic;
using static Vodovoz.Tools.Interactive.ConfirmationQuestion.ConfirmationQuestionDialogSettings;

namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public interface IConfirmationQuestionInteractive
	{
		bool Question(
			IEnumerable<ConfirmationQuestion> questions,
			string title = null,
			string topText = null,
			string bottomText = null,
			bool isYesButtonAvailableByDefault = false,
			bool isNoButtonAvailableByDefault = false,
			ImgType imageType = ImgType.Question);
	}
}
