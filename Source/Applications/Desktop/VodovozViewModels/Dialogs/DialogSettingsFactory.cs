using QS.Project.Services.FileDialog;

namespace Vodovoz.ViewModels.Dialogs
{
	public class DialogSettingsFactory
	{
		public DialogSettings CreateForExcelExport()
		{
			return new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx"
			};
		}
	}
}
