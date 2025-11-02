using QS.Project.Services.FileDialog;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.Presentation.ViewModels.Factories
{
	public interface IDialogSettingsFactory
	{
		DialogSettings CreateForClosedXmlReport(IClosedXmlReport closedXmlReport, string fileName = null);
	}
}
