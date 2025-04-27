namespace Vodovoz.Presentation.ViewModels.Common
{
	public class TextChangeEventArgs
	{
		public string OldText { get; }
		public string NewText { get; }

		public TextChangeEventArgs(string oldText, string newText)
		{
			OldText = oldText;
			NewText = newText;
		}
	}
}
