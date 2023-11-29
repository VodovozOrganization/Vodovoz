using QS.Dialog;

namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public class ConfirmationQuestionDialogSettings
	{
		public string Title { get; set; }
		public string TopText { get; set; }
		public string BottomText { get; set; }
		public bool IsYesButtonAvailableByDefault { get; set; }
		public bool IsNoButtonAvailableByDefault { get; set; }
		public DialogPurpose Purpose { get; set; }

		public enum DialogPurpose
		{
			Question,
			Info,
			Warning,
			Error
		}
	}
}
