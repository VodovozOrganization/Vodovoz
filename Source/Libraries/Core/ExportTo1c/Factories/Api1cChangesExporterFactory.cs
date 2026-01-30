using ExportTo1c.Library.Exporters;
using System;

namespace ExportTo1c.Library.Factories
{
	public class Api1cChangesExporterFactory : IApi1cChangesExporterFactory
	{
		public Api1cChangesExporter CreateApi1cDataExporter(DateTime fromDate)
		{
			return new Api1cChangesExporter(fromDate);
		}
	}
}
