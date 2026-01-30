using ExportTo1c.Library.Exporters;
using System;

namespace ExportTo1c.Library.Factories
{
	public interface IApi1cChangesExporterFactory
	{
		Api1cChangesExporter CreateApi1cDataExporter(DateTime fromDate);
	}
}
