using ClosedXML.Report;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.Extensions
{
	public static class ClosedXmlReportExtensions
	{
		public static void Export(this IClosedXmlReport closedXmlReport, string path, bool adjustToContents = true)
		{
			var template = new XLTemplate(closedXmlReport.TemplatePath);
			template.AddVariable(closedXmlReport);
			template.Generate();

			if(adjustToContents)
			{
				foreach(var worksheet in template.Workbook.Worksheets)
				{
					foreach(var column in worksheet.Columns())
					{
						column.AdjustToContents();
					}

					foreach(var row in worksheet.Rows())
					{
						row.AdjustToContents();
						row.ClearHeight();
					}
				}
			}

			template.SaveAs(path);
		}
	}
}
