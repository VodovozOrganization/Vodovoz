using QS.Dialog;
using Vodovoz.Services;

namespace Vodovoz.ServiceDialogs
{
	public class CustomInteractiveMessage : IInteractiveMessage
	{
		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			switch(level)
			{
				case ImportanceLevel.Error:
					CastomMessageDialogHelper.RunErrorDialog(message, title);
					break;
				case ImportanceLevel.Warning:
					CastomMessageDialogHelper.RunWarningDialog(message, title);
					break;
				case ImportanceLevel.Info:
				default:
					CastomMessageDialogHelper.RunInfoDialog(message, title);
					break;
			}
		}
	}
}
