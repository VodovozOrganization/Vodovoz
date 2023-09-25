using QS.Dialog;

namespace Vodovoz.ServiceDialogs
{
	public class CustomInteractiveService : IInteractiveService
	{
		private readonly CustomInteractiveMessage _interactiveMessage = new CustomInteractiveMessage();
		private readonly CustomQuestion _question = new CustomQuestion();

		public bool Question(string message, string title = null)
		{
			return _question.Question(message, title);
		}

		public string Question(string[] buttons, string message, string title = null)
		{
			return _question.Question(buttons, message, title);
		}

		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			_interactiveMessage.ShowMessage(level, message, title);
		}
	}
}
