using Microsoft.VisualBasic.FileIO;
using QS.DomainModel.Entity;
using QS.Project.Services.FileDialog;
using RestSharp.Extensions;
using System;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.ViewModels.Factories
{
	internal class DialogSettingsFactory : IDialogSettingsFactory
	{
		public DialogSettings CreateForClosedXmlReport(IClosedXmlReport closedXmlReport, string fileName = null)
		{
			var reportName = fileName ?? closedXmlReport.GetType().GetAttribute<AppellativeAttribute>().Nominative;

			var exportDate = DateTime.Now.ToString("yyyy-MM-dd HH-mm");

			var reportFileExtension = ".xlsx";

			var settings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = reportFileExtension,
				InitialDirectory = SpecialDirectories.Desktop,
				FileName = $"{reportName} {exportDate}{reportFileExtension}"
			};

			settings.FileFilters.Clear();
			settings.FileFilters.Add(new DialogFileFilter($"Отчет Excel", "*" + reportFileExtension));

			return settings;
		}
	}
}
