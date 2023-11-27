namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public interface IConfirmationQuestionInteractive
	{
		bool Question(ConfirmationQuestionDialogInfo dialogInfo, ConfirmationQuestion question1, ConfirmationQuestion question2 = null, ConfirmationQuestion question3 = null);
	}
}
