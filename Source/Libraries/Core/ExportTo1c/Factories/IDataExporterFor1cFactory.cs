using ExportTo1c.Library.Exporters;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Factories
{
	public interface IDataExporterFor1cFactory
	{
		IDataExporterFor1c Create1cDataExporter(Export1cMode export1CMode);
	}
}
