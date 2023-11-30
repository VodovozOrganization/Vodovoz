using QS.Dialog;

namespace Vodovoz.Tools.Interactive.ConfirmationQuestion
{
	public partial class ConfirmationQuestionDialogSettings
	{
		public string Title { get; set; }
		public string TopText { get; set; }
		public string BottomText { get; set; }
		public bool IsYesButtonAvailableByDefault { get; set; }
		public bool IsNoButtonAvailableByDefault { get; set; }
		public ImgType ImageType { get; set; }
	}
}
