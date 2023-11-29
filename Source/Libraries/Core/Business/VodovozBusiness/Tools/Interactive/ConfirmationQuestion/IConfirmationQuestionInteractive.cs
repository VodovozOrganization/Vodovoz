namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public interface IConfirmationQuestionInteractive
	{
		bool Question(ConfirmationQuestionDialogSettings dialogSettings, params ConfirmationQuestion[] questions);
	}
}
