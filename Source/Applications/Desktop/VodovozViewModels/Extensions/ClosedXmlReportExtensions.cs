﻿using ClosedXML.Report;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.Extensions
{
	public static class ClosedXmlReportExtensions
	{
		public static XLTemplate RenderTemplate(this IClosedXmlReport closedXmlReport, bool adjustToContents = true)
		{
			var template = GetRawTemplate(closedXmlReport);
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

			return template;
		}

		public static XLTemplate RenderTemplate(this XLTemplate template, IClosedXmlReport closedXmlReport, bool adjustToContents = true)
		{
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

			return template;
		}

		public static XLTemplate GetRawTemplate(this IClosedXmlReport closedXmlReport)
		{
			return new XLTemplate(closedXmlReport.TemplatePath);
		}

		public static void Export(this IClosedXmlReport closedXmlReport, string path, bool adjustToContents = true)
		{
			var renderedTemplate = RenderTemplate(closedXmlReport, adjustToContents);

			renderedTemplate.Export(path);
		}

		public static void Export(this XLTemplate template, string path)
		{
			template.SaveAs(path);
		}
	}
}
