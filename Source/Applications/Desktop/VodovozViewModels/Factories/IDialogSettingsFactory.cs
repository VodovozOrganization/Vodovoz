using QS.Project.Services.FileDialog;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.Factories
{
	public interface IDialogSettingsFactory
	{
		DialogSettings CreateForClosedXmlReport(IClosedXmlReport closedXmlReport);
	}
}
