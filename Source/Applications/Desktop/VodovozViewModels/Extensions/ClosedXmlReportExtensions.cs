using ClosedXML.Report;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.Extensions
{
	public static class ClosedXmlReportExtensions
	{
		public static void Export(this IClosedXmlReport closedXmlReport, string path)
		{
			var template = new XLTemplate(closedXmlReport.TemplatePath);
			template.AddVariable(closedXmlReport);
			template.Generate();
			template.SaveAs(path);
		}
	}
}
