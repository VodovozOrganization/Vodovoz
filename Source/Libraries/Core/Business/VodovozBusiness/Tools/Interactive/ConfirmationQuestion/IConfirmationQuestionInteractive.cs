namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public interface IConfirmationQuestionInteractive
	{
		bool Question(ConfirmationQuestionDialogSettings dialogInfo, params ConfirmationQuestion[] questions);
	}
}
