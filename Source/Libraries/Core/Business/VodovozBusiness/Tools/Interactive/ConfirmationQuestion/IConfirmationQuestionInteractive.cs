namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public interface IConfirmationQuestionInteractive
	{
		bool Question(ConfirmationQuestionDialogInfo dialogInfo, params ConfirmationQuestion[] questions);
	}
}
