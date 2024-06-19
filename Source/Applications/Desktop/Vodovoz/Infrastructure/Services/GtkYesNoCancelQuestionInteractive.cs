using QS.Dialog.GtkUI;
using Vodovoz.Tools.Interactive.YesNoCancelQuestion;

namespace Vodovoz.Infrastructure.Services
{
	internal class GtkYesNoCancelQuestionInteractive : IYesNoCancelQuestionInteractive
	{
		public YesNoCancelQuestionResult Question(string question)
		{
			var result = MessageDialogHelper.RunQuestionYesNoCancelDialog(question);

			switch(result)
			{
				case -8:
					return YesNoCancelQuestionResult.Yes;
				case -9:
					return YesNoCancelQuestionResult.No;
				case -6:
				default:
					return YesNoCancelQuestionResult.Cancel;

			}
		}
	}
}
